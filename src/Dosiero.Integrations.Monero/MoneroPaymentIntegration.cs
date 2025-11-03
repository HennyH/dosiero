using Dosiero.Abstractions.Payments;
using Dosiero.Integrations.Monero.Slices;
using Dosiero.UriUtility;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.WebUtilities;

using Monero.WalletRpc;

using RazorSlices;

using static Monero.WalletRpc.CreateAddress;
using static Monero.WalletRpc.GetAddress;
using static Monero.WalletRpc.GetAddressIndex;
using static Monero.WalletRpc.GetTransfers;
using static Monero.WalletRpc.Sign;
using static Monero.WalletRpc.Verify;

namespace Dosiero.Integrations.Monero;

file static class UriParams
{
    public const string TxAmount = "ta";
    public const string TxDescription = "td";
    public const string FileUri = "fu";
    public const string Signature = "sg";
}

public sealed record MoneroPayment(string Address, decimal TxAmount, string? TxDescription, Uri FileUri) : IPayment
{
    public Uri TransferUri => Uri.From($"monero:{Address}?{TxAmount:tx_amount}&{TxDescription:tx_description}");

    public Uri Uri => Uri.From($"monero:{Address}?{Uri.Param(UriParams.TxAmount, TxAmount)}&{Uri.Param(UriParams.TxDescription, TxDescription)}&{Uri.Param(UriParams.FileUri, FileUri)}");
}

public sealed record SignedMoneroPayment(MoneroPayment MoneroPayment, string Signature) : IPayment
{
    public Uri FileUri => MoneroPayment.FileUri;

    public Uri TransferUri => MoneroPayment.TransferUri;

    public Uri Uri => Uri.From($"{Uri.Raw(MoneroPayment.Uri)}&{Uri.Param(UriParams.Signature, Signature)}");
}

internal sealed class MoneroPaymentIntegration(IWalletRpcClient wallet)
    : IPaymentIntegration<MoneroPaymentIntegration, SignedMoneroPayment> 
{
    public static string Scheme => "monero";

    public async Task<SignedMoneroPayment> CreatePaymentAsync(CreatePaymentParameters parameters, CancellationToken token = default)
    {
        CreateAddressResult address;
        {
            var response = await wallet.CallAsync(new CreateAddressParameters
            {
                AccountIndex = 0,
                Label = $"Payment - For {parameters.FileUri} ({parameters.FilePrice.Price} XMR)"
            });

            if (!response.IsOk)
            {
                throw new Exception($"Monero RPC Error ({response.Error.Code}): {response.Error.Message}");
            }

            address = response.Result;
        }

        var payment = new MoneroPayment(address.Address, parameters.FilePrice.Price, $"Payment for {parameters.FileUri}", parameters.FileUri);

        string signature;
        {
            var response = await wallet.CallAsync(new SignParameters
            {
                Data = payment.Uri.ToString()
            });

            if (!response.IsOk)
            {
                throw new Exception($"Monero RPC Error ({response.Error.Code}): {response.Error.Message}");
            }

            signature = response.Result.Signature;
        }

        return new SignedMoneroPayment(payment, signature);
    }

    public async Task<bool> HasPaymentBeenMadeAsync(SignedMoneroPayment payment, CancellationToken token = default)
    {
        GetAddressIndex.AddressIndex index;
        {
            var response = await wallet.CallAsync(new GetAddressIndexParameters { Address = payment.MoneroPayment.Address }, token: token);

            if (!response.IsOk)
            {
                throw new ArgumentException($"Unrecognised address {payment.MoneroPayment.Address}.");
            }

            index = response.Result.Index;
        }

        GetTransfers.Transfer[] transfers;
        {
            var response = await wallet.CallAsync(new GetTransfersParameters
            {
                In = true,
                Pending = true,
                Pool = true,
                AccountIndex = index.Major,
                SubaddressIndices = [index.Minor]
            }, token: token);

            response.ThrowIfError();

            transfers = [ ..response.Result.In, ..response.Result.Pending, ..response.Result.Pool ];
        }

        ulong totalAmount = 0;
        foreach (var transfer in transfers)
        {
            if (transfer is { Destinatations: [] destinations })
            {
                foreach (var destination in destinations)
                {
                    if (destination.Address != payment.MoneroPayment.Address)
                    {
                        totalAmount += destination.Amount;
                    }
                }
            }
            else if (transfer.Address == payment.MoneroPayment.Address)
            {
                totalAmount += transfer.Amount;
            }
        }

        var totalXmr = totalAmount.FromAtomicUnitsToXmr();
        return totalXmr >= payment.MoneroPayment.TxAmount;
    }

    public async Task<IHtmlContent> RenderPaymentUi(SignedMoneroPayment payment, CancellationToken token = default)
    {
        return new HtmlString(await MoneroPaymentUi.Create(payment).RenderAsync(cancellationToken: token));
    }

    public async Task<SignedMoneroPayment> FromUriAsync(Uri uri, CancellationToken token = default)
    {
        if (!string.Equals(uri.Scheme, Scheme, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Cannot recover monero payment from scheme {uri.Scheme}.");
        }

        var address = uri.AbsolutePath;

        var query = QueryHelpers.ParseQuery(uri.Query);

        if (!query.TryGetValue(UriParams.TxAmount, out var txAmountTextValues))
        {
            throw new ArgumentException($"Required query parameter '{UriParams.TxAmount}' not found.");
        }

        if (!decimal.TryParse(txAmountTextValues, out var txAmount))
        {
            throw new ArgumentException($"Query parameter '{UriParams.TxAmount}' value {txAmountTextValues} could not be parsed as a decimal.");
        }

        _ = query.TryGetValue(UriParams.TxDescription, out var txDescriptionValues);

        if (!query.TryGetValue(UriParams.FileUri, out var fileUriValues))
        {
            throw new ArgumentException($"Required query parameter '{UriParams.FileUri}' not found.");
        }

        if (string.IsNullOrWhiteSpace(fileUriValues))
        {
            throw new ArgumentException($"Required query parameter '{UriParams.FileUri}' cannnot be empty.");
        }

        if (!Uri.TryCreate(fileUriValues, UriKind.Absolute, out var fileUri))
        {
            throw new ArgumentException($"Unrecognised file URI {fileUriValues}.");
        }

        if (!query.TryGetValue(UriParams.Signature, out var signatureValues))
        {
            throw new ArgumentException($"Required query parameter '{UriParams.Signature}' not found.");
        }

        string? signature = signatureValues;

        if (string.IsNullOrWhiteSpace(signature))
        {
            throw new ArgumentException($"Required query parameter '{UriParams.Signature}' cannnot be empty.");
        }

        {
            var response = await wallet.CallAsync(new GetAddressIndexParameters { Address = address }, token: token);

            if (!response.IsOk)
            {
                throw new ArgumentException($"Unrecognised address {address}.");
            }
        }

        string publicAddress;
        {
            var response = await wallet.CallAsync(new GetAddressParameters
            {
                AccountIndex = 0,
                AddressIndices = [0]
            });

            response.ThrowIfError();

            publicAddress = response.Result.Address;
        }

        var payment = new MoneroPayment(address, txAmount, txDescriptionValues, fileUri);

        {
            var response = await wallet.CallAsync(new VerifyParameters
            {
                Address = publicAddress,
                Data = payment.Uri.ToString(),
                Signature = signature
            }, token: token);

            if (!response.IsOk)
            {
                throw new Exception($"Monero RPC Error ({response.Error.Code}): {response.Error.Message}");
            }

            if (!response.Result.Good)
            {
                throw new ArgumentException("Invalid signature");
            }
        }

        return new SignedMoneroPayment(payment, signature);
    }

    public Task<Uri> ToUriAsync(SignedMoneroPayment payment, CancellationToken token = default)
        => Task.FromResult(payment.Uri);
}


file static class MoneroExtensions
{
    public static decimal FromAtomicUnitsToXmr(this ulong atomicUnits)
    {
        const ulong ATOMIC_UNITS_PER_XMR = 1000000000000;

        return atomicUnits / (decimal)ATOMIC_UNITS_PER_XMR;
    }
}
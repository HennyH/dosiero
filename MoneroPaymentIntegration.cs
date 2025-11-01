using Dosiero.Components;
using Dosiero.MoneroRpc;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

using static Dosiero.MoneroRpc.CreateAddress;
using static Dosiero.MoneroRpc.GetAddress;
using static Dosiero.MoneroRpc.GetAddressIndex;
using static Dosiero.MoneroRpc.GetTransfers;
using static Dosiero.MoneroRpc.Sign;
using static Dosiero.MoneroRpc.Verify;

namespace Dosiero;

file static class UriParams
{
    public const string TxAmount = "ta";
    public const string TxDescription = "td";
    public const string FileName = "fn";
    public const string Id = "id";
    public const string Signature = "sg";
}

public sealed record MoneroPayment(string Id, string Address, decimal TxAmount, string? TxDescription, string FileName) : IPayment
{
    public Uri TransferUri => Uri.From($"monero:{Address}?{TxAmount:tx_amount}&{TxDescription:tx_description}");

    public Uri Uri => Uri.From($"monero:{Address}?{new UrlParam(UriParams.TxAmount, TxAmount)}&{new UrlParam(UriParams.TxDescription, TxDescription)}&{new UrlParam(UriParams.FileName, FileName)}&{new UrlParam(UriParams.Id, Id)}");
}

public sealed record SignedMoneroPayment(MoneroPayment MoneroPayment, string Signature) : IPayment
{
    public string Id => MoneroPayment.Id;

    public string FileName => MoneroPayment.FileName;

    public Uri TransferUri => MoneroPayment.TransferUri;

    public Uri Uri => Uri.From($"{new UrlRaw(MoneroPayment.Uri)}&{new UrlParam(UriParams.Signature, Signature)}");
}

public sealed class MoneroPaymentIntegration(WalletRpcClient wallet, IServiceScopeFactory serviceScopeFactory) : IPaymentIntegration<SignedMoneroPayment>
{
    public string Scheme => "monero";

    public async Task<SignedMoneroPayment> CreatePaymentAsync(CreatePaymentParameters parameters, CancellationToken token = default)
    {
        CreateAddressResult address;
        {
            var response = await wallet.CallAsync(new CreateAddressParameters
            {
                AccountIndex = 0,
                Label = $"Payment {parameters.PaymentId} - For {parameters.FileName} ({parameters.FilePrice.Price} XMR)"
            });

            if (!response.IsOk)
            {
                throw new Exception($"Monero RPC Error ({response.Error.Code}): {response.Error.Message}");
            }

            address = response.Result;
        }

        var payment = new MoneroPayment(parameters.PaymentId, address.Address, parameters.FilePrice.Price, $"Payment for {parameters.FileName}", parameters.FileName);

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
        GetAddressIndexResult.AddressIndex index;
        {
            var response = await wallet.CallAsync(new GetAddressIndexParameters { Address = payment.MoneroPayment.Address });

            if (!response.IsOk)
            {
                throw new ArgumentException($"Unrecognised address {payment.MoneroPayment.Address}.");
            }

            index = response.Result.Index;
        }

        GetTransfersResult.Transfer[] transfers;
        {
            var response = await wallet.CallAsync(new GetTransfersParameters
            {
                In = true,
                Pending = true,
                Pool = true,
                AccountIndex = index.Major,
                SubaddressIndices = [index.Minor]
            });

            /* TODO: Replace with utility function to throw if not success */
            if (!response.IsOk)
            {
                throw new Exception($"Monero RPC Error ({response.Error.Code}): {response.Error.Message}");
            }

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

    public async Task<MarkupString> RenderPaymentUi(SignedMoneroPayment payment, CancellationToken token = default)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var html = await BlazorRenderer.RenderToStringAsync<MoneroPaymentUi>(
            scope,
            navigationInit: NavigationInit.None,
            parameters: ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                { "Payment", payment }
            }));
        return new MarkupString(html);
    }

    public async Task<SignedMoneroPayment> FromUriAsync(Uri uri, CancellationToken token = default)
    {
        if (!string.Equals(uri.Scheme, Scheme, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Cannot recover monero payment from scheme {uri.Scheme}.");
        }

        var address = uri.AbsolutePath;

        var query = QueryHelpers.ParseQuery(uri.Query);

        if (!query.TryGetValue(UriParams.Id, out var idValues))
        {
            throw new ArgumentException($"Required query parameter '{UriParams.Id}' not found.");
        }

        if (!query.TryGetValue(UriParams.TxAmount, out var txAmountTextValues))
        {
            throw new ArgumentException($"Required query parameter '{UriParams.TxAmount}' not found.");
        }

        if (!decimal.TryParse(txAmountTextValues, out var txAmount))
        {
            throw new ArgumentException($"Query parameter '{UriParams.TxAmount}' value {txAmountTextValues} could not be parsed as a decimal.");
        }

        _ = query.TryGetValue(UriParams.TxDescription, out var txDescriptionValues);

        if (!query.TryGetValue(UriParams.FileName, out var fileNameValues))
        {
            throw new ArgumentException($"Required query parameter '{UriParams.FileName}' not found.");
        }

        if (!query.TryGetValue(UriParams.Signature, out var signatureValues))
        {
            throw new ArgumentException($"Required query parameter '{UriParams.Signature}' not found.");
        }

        string? id = idValues;
        string? fileName = fileNameValues;
        string? signature = signatureValues;

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException($"Required query parameter '{UriParams.Signature}' cannnot be empty.");
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException($"Required query parameter '{UriParams.FileName}' cannnot be empty.");
        }

        if (string.IsNullOrWhiteSpace(signature))
        {
            throw new ArgumentException($"Required query parameter '{UriParams.Signature}' cannnot be empty.");
        }

        {
            var response = await wallet.CallAsync(new GetAddressIndexParameters { Address = address });

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

            /* TODO: Replace with utility function to throw if not success */
            if (!response.IsOk)
            {
                throw new Exception($"Monero RPC Error ({response.Error.Code}): {response.Error.Message}");
            }

            publicAddress = response.Result.Address;
        }

        var payment = new MoneroPayment(id, address, txAmount, txDescriptionValues, fileName);

        {
            var response = await wallet.CallAsync(new VerifyParameters
            {
                Address = publicAddress,
                Data = payment.Uri.ToString(),
                Signature = signature
            });

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
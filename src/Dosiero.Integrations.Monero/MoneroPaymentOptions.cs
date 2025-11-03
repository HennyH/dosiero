using Monero.WalletRpc;

using System.ComponentModel.DataAnnotations;

namespace Dosiero.Integrations.Monero;

public sealed class MoneroPaymentOptions
{
    public const string MoneroPayment = nameof(MoneroPayment);

    [Required]
    public required Uri WalletRpcUri { get; set; }

    public string? WalletRpcUsername { get; set; }

    public string? WalletRpcPassword { get; set; }

    public bool AcceptSelfSignedCerts { get; set; } = true;
}

internal static class MoneroPaymentOptionsExtensions
{
    public static WalletRpcOptions ToWalletRpcOptions(this MoneroPaymentOptions options)
    {
        return new WalletRpcOptions
        {
            Uri = options.WalletRpcUri,
            Username = options.WalletRpcUsername,
            Password = options.WalletRpcPassword,
            AcceptSelfSignedCerts = options.AcceptSelfSignedCerts
        };
    }
}
using Monero.WalletRpc;

using System.ComponentModel.DataAnnotations;

namespace Dosiero.Integrations.Monero;

public sealed class MoneroPaymentOptions
{
    public const string MoneroPayment = nameof(MoneroPayment);

    [Required]
    public required Uri Url { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public bool AcceptSelfSignedCerts { get; set; } = true;
}

internal static class MoneroPaymentOptionsExtensions
{
    public static WalletRpcOptions ToWalletRpcOptions(this MoneroPaymentOptions options)
    {
        return new WalletRpcOptions
        {
            Uri = options.Url,
            Login = options is { Username: { } username, Password: { } password }
                ? new RpcLogin { Username = username, Password = password }
                : default,
            AcceptSelfSignedCerts = options.AcceptSelfSignedCerts
        };
    }
}
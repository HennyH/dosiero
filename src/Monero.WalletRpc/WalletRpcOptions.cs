using Microsoft.Extensions.Options;

using System.ComponentModel.DataAnnotations;

namespace Monero.WalletRpc;

public sealed class RpcLogin
{
    public required string Username { get; set; }

    public required string Password { get; set; }
}

public sealed class WalletRpcOptions
{
    public const string WalletRpc = nameof(WalletRpcOptions);

    [Required]
    public required Uri Uri { get; set; }

    public RpcLogin? Login { get; set; }

    public bool AcceptSelfSignedCerts { get; set; } = true;
}

[OptionsValidator]
internal partial class WalletRpcOptionsValidator : IValidateOptions<WalletRpcOptions>;
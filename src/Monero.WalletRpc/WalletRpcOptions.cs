using Microsoft.Extensions.Options;

using System.ComponentModel.DataAnnotations;

namespace Monero.WalletRpc;

public sealed class WalletRpcOptions
{
    public const string WalletRpc = nameof(WalletRpcOptions);

    [Required]
    public required Uri Uri { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set;  }

    public bool AcceptSelfSignedCerts { get; set; } = true;
}

[OptionsValidator]
internal partial class WalletRpcOptionsValidator : IValidateOptions<WalletRpcOptions>;
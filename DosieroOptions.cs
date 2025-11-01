namespace Dosiero;

public sealed class DosieroOptions
{
    public const string Dosiero = "Dosiero";

    public required string Folder { get; set; }

    public required Uri WalletRpcUri { get; set; }

    public string? WalletRpcUserName { get; set; }

    public string? WalletRpcPassword { get; set; }

    public bool? WalletRpcAcceptSelfSignedCerts { get; set; } = true;
}
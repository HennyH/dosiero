using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Monero.WalletRpc;

public sealed record RpcResponse<T>
    where T : notnull
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("result")]
    public T? Result { get; set; }

    [JsonPropertyName("error")]
    public RpcError? Error { get; set; }

    [MemberNotNullWhen(true, nameof(Result)), MemberNotNullWhen(false, nameof(Error))]
    public bool IsOk => Result is not null;

    [MemberNotNull(nameof(Result))]
    public void ThrowIfError()
    {
        if (this.IsOk)
        {
            return;
        }

        throw new MoneroRpcErrorException(Error);
    }
}

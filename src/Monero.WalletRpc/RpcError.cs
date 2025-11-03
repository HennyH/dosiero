using System.Text.Json.Serialization;

namespace Monero.WalletRpc;

public sealed record RpcError
{
    [JsonPropertyName("code")]
    public required int Code { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; }
}

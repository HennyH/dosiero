using System.Text.Json.Serialization;

namespace Monero.WalletRpc;

public sealed record RpcRequest<T>
    where T : notnull
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("method")]
    public required string Method { get; set; }

    [JsonPropertyName("params")]
    public required T Params { get; set; }
}

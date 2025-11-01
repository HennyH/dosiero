using System.Text.Json.Serialization;

namespace Dosiero.MoneroRpc;

public sealed record RpcError
{
    [JsonPropertyName("code")]
    public required int Code { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; }
}

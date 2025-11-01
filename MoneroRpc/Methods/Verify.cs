using System.Text.Json.Serialization;

using static Dosiero.MoneroRpc.Verify;

namespace Dosiero.MoneroRpc;

public sealed class Verify : IRpcMethod<Verify, VerifyParameters, VerifyResult>
{
    public static string MethodName { get; } = "verify";

    public sealed record VerifyParameters
    {
        [JsonPropertyName("data")]
        public required string Data { get; set; }

        [JsonPropertyName("address")]
        public required string Address { get; set; }

        [JsonPropertyName("signature")]
        public required string Signature { get; set; }
    }

    public sealed record VerifyResult
    {
        [JsonPropertyName("good")]
        public required bool Good { get; set; }
    }
}

public sealed partial class WalletRpcClient : IRpcMethodImpl<Verify, VerifyParameters, VerifyResult>
{
    public Task<RpcResponse<VerifyResult>> CallAsync(VerifyParameters paramters, string? id = null, CancellationToken token = default)
        => CallAsync<Verify, VerifyParameters, VerifyResult>(paramters, id, token);
}

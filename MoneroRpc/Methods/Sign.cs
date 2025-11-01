using System.Text.Json.Serialization;

using static Dosiero.MoneroRpc.Sign;

namespace Dosiero.MoneroRpc;

public sealed class Sign : IRpcMethod<Sign, SignParameters, SignResult>
{
    public static string MethodName { get; } = "sign";
 
    public sealed record SignParameters
    {
        [JsonPropertyName("data")]
        public required string Data { get; set; }
    }

    public sealed record SignResult
    {
        [JsonPropertyName("signature")]
        public required string Signature { get; set; }
    }
}

public sealed partial class WalletRpcClient : IRpcMethodImpl<Sign, SignParameters, SignResult>
{
    public Task<RpcResponse<SignResult>> CallAsync(SignParameters paramters, string? id = null, CancellationToken token = default)
        => CallAsync<Sign, SignParameters, SignResult>(paramters, id, token);
}

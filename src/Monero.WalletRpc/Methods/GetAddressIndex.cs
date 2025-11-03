using System.Text.Json.Serialization;

using static Monero.WalletRpc.GetAddressIndex;

namespace Monero.WalletRpc;

public sealed class GetAddressIndex : IRpcMethod<GetAddressIndex, GetAddressIndexParameters, GetAddressIndexResult>
{
    public static string MethodName { get; } = "get_address_index";

    public sealed record GetAddressIndexParameters
    {
        [JsonPropertyName("address")]
        public required string Address { get; set; }
    }

    public sealed record GetAddressIndexResult
    {
        [JsonPropertyName("index")]
        public required AddressIndex Index { get; set; }
    }

    public sealed record AddressIndex
    {
        [JsonPropertyName("major")]
        public required ulong Major { get; set; }

        [JsonPropertyName("minor")]
        public required ulong Minor { get; set; }
    }
}

public partial interface IWalletRpcClient : IRpcMethodImpl<GetAddressIndex, GetAddressIndexParameters, GetAddressIndexResult>;

internal sealed partial class WalletRpcClient
{
    public Task<RpcResponse<GetAddressIndexResult>> CallAsync(GetAddressIndexParameters paramters, string? id = null, CancellationToken token = default)
        => CallAsync<GetAddressIndex, GetAddressIndexParameters, GetAddressIndexResult>(paramters, id, token);
}

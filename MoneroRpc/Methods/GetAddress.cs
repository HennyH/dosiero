using System.Text.Json.Serialization;

using static Dosiero.MoneroRpc.GetAddress;

namespace Dosiero.MoneroRpc;

public sealed class GetAddress : IRpcMethod<GetAddress, GetAddressParameters, GetAddressResult>
{
    public static string MethodName { get; } = "get_address";

    public sealed record GetAddressParameters
    {
        [JsonPropertyName("account_index")]
        public required ulong AccountIndex { get; set; }

        [JsonPropertyName("address_index")]
        public ulong[]? AddressIndices { get; set; }
    }

    public sealed record GetAddressResult
    {
        [JsonPropertyName("address")]
        public required string Address { get; set; }

        [JsonPropertyName("addresses")]
        public required AddressEnty[] Addresses { get; set; }

        public sealed record AddressEnty
        {
            [JsonPropertyName("address")]
            public required string Address { get; set; }

            [JsonPropertyName("address_index")]
            public required ulong AddressIndex { get; set; }

            [JsonPropertyName("label")]
            public string? Label
            {
                get => field;
                set => field = string.IsNullOrWhiteSpace(value) ? null : value;
            }

            [JsonPropertyName("used")]
            public bool Used { get; set; }
        }
    }
}

public sealed partial class WalletRpcClient : IRpcMethodImpl<GetAddress, GetAddressParameters, GetAddressResult>
{
    public Task<RpcResponse<GetAddressResult>> CallAsync(GetAddressParameters paramters, string? id = null, CancellationToken token = default)
        => CallAsync<GetAddress, GetAddressParameters, GetAddressResult>(paramters, id, token);
}

using System.Text.Json.Serialization;

using static Dosiero.MoneroRpc.CreateAddress;

namespace Dosiero.MoneroRpc;

public sealed class CreateAddress : IRpcMethod<CreateAddress, CreateAddressParameters, CreateAddressResult>
{
    public static string MethodName { get; } = "create_address";

    public sealed record CreateAddressParameters
    {
        [JsonPropertyName("account_index")]
        public required ulong AccountIndex { get; set; }

        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; } = 1;
    }

    public sealed record CreateAddressResult
    {
        [JsonPropertyName("address")]
        public required string Address { get; set; }

        [JsonPropertyName("address_index")]
        public required ulong AddressIndex { get; set; }

        [JsonPropertyName("address_indices")]
        public required ulong[] AddressIndices { get; set; }

        [JsonPropertyName("addresses")]
        public required string[] Addresses { get; set; }
    }
}

public sealed partial class WalletRpcClient : IRpcMethodImpl<CreateAddress, CreateAddressParameters, CreateAddressResult>
{
    public Task<RpcResponse<CreateAddressResult>> CallAsync(CreateAddressParameters paramters, string? id = null, CancellationToken token = default)
        => CallAsync<CreateAddress, CreateAddressParameters, CreateAddressResult>(paramters, id, token);
}

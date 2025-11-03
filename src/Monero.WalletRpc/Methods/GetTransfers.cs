using System.Text.Json.Serialization;

using static Monero.WalletRpc.GetTransfers;

namespace Monero.WalletRpc;

public sealed class GetTransfers : IRpcMethod<GetTransfers, GetTransfersParameters, GetTransfersResult>
{
    public static string MethodName { get; } = "get_transfers";

    public sealed record GetTransfersParameters
    {
        [JsonPropertyName("in")]
        public bool? In { get; set; }

        [JsonPropertyName("out")]
        public bool? Out { get; set; }

        [JsonPropertyName("pending")]
        public bool? Pending { get; set; }

        [JsonPropertyName("failed")]
        public bool? Failed { get; set; }

        [JsonPropertyName("pool")]
        public bool? Pool { get; set; }

        [JsonPropertyName("filter_by_height")]
        public bool? FilterByHeight { get; set; }

        [JsonPropertyName("min_height")]
        public ulong? MinHeight { get; set; }

        [JsonPropertyName("max_height")]
        public ulong? MaxHeight { get; set; }

        [JsonPropertyName("account_index")]
        public ulong? AccountIndex { get; set; }

        [JsonPropertyName("subaddr_indices")]
        public ulong[]? SubaddressIndices { get; set; }

        [JsonPropertyName("all_accounts")]
        public bool? AllAccounts { get; set; }
    }

    public sealed record GetTransfersResult
    {
        [JsonPropertyName("in")]
        public Transfer[] In { get; set; } = [];

        [JsonPropertyName("out")]
        public Transfer[] Out { get; set; } = [];

        [JsonPropertyName("pending")]
        public Transfer[] Pending { get; set; } = [];

        [JsonPropertyName("failed")]
        public Transfer[] Failed { get; set; } = [];

        [JsonPropertyName("pool")]
        public Transfer[] Pool { get; set; } = [];

    }

    public sealed record Transfer
    {
        [JsonPropertyName("address")]
        public required string Address { get; set; }

        [JsonPropertyName("amount")]
        public required ulong Amount { get; set; }

        [JsonPropertyName("amounts")]
        public required ulong[] Amounts { get; set; }

        [JsonPropertyName("confirmations")]
        public ulong? Confirmations { get; set; }

        [JsonPropertyName("double_spend_seen")]
        public required bool DoubleSpendSeen { get; set; }

        [JsonPropertyName("fee")]
        public required ulong Fee { get; set; }

        [JsonPropertyName("height")]
        public ulong Height { get; set; }

        [JsonPropertyName("note")]
        public string? Note
        {
            get => field;
            set => field = string.IsNullOrWhiteSpace(value) ? null : value;
        }

        [JsonPropertyName("destinations")]
        public Destination[]? Destinatations { get; set; }

        [JsonPropertyName("payment_id")]
        public required string PaymentId { get; set; }

        [JsonPropertyName("subaddr_index")]
        public required AddressIndex SubaddressIndex { get; set; }

        [JsonPropertyName("subaddr_indices")]
        public required AddressIndex[] SubaddressIndicies { get; set; }

        [JsonPropertyName("suggested_confirmations_threshold")]
        public required ulong SuggestedConfirmationsThreshold { get; set; }

        [JsonPropertyName("timestamp")]
        public required ulong Timestamp { get; set; }

        [JsonPropertyName("txid")]
        public required string TransactionId { get; set; }

        [JsonPropertyName("type")]
        public required string Type { get; set; }

        [JsonPropertyName("unlock_time")]
        public required ulong UnlockTime { get; set; }

        [JsonPropertyName("locked")]
        public required bool Locked { get; set; }
    }

    public sealed record Destination
    {
        [JsonPropertyName("amount")]
        public required ulong Amount { get; set; }

        [JsonPropertyName("address")]
        public required string Address { get; set; }
    }

    public sealed record AddressIndex
    {
        [JsonPropertyName("major")]
        public required ulong Major { get; set; }

        [JsonPropertyName("minor")]
        public required ulong Minor { get; set; }
    }

}

public partial interface IWalletRpcClient : IRpcMethodImpl<GetTransfers, GetTransfersParameters, GetTransfersResult>;

internal sealed partial class WalletRpcClient
{
    public Task<RpcResponse<GetTransfersResult>> CallAsync(GetTransfersParameters paramters, string? id = null, CancellationToken token = default)
        => CallAsync<GetTransfers, GetTransfersParameters, GetTransfersResult>(paramters, id, token);
}

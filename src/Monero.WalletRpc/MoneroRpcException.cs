namespace Monero.WalletRpc;

public sealed class MoneroRpcErrorException(RpcError error)
    : Exception($"Monero RPC error ({error.Code}): {error.Message}")
{
    public RpcError RpcError => error;
}

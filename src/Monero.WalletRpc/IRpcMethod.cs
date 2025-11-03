namespace Monero.WalletRpc;

public interface IRpcMethod<TMethod, TParameters, TResult>
    where TMethod : IRpcMethod<TMethod, TParameters, TResult>
    where TParameters : notnull
    where TResult : notnull
{
    public static abstract string MethodName { get; }
}

public interface IRpcMethodImpl<TMethod, TParameters, TResult>
    where TMethod : IRpcMethod<TMethod, TParameters, TResult>
    where TParameters : notnull
    where TResult : notnull
{
    public abstract Task<RpcResponse<TResult>> CallAsync(TParameters paramters, string? id = null, CancellationToken token = default);
}
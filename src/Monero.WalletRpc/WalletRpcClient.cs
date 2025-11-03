using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace Monero.WalletRpc;

internal partial class WalletRpcClient(HttpClient http) : IWalletRpcClient
{
    private async Task<RpcResponse<TResult>> CallAsync<TMethod, TParameters, TResult>(TParameters parameters, string? id = null, CancellationToken token = default)
        where TMethod : IRpcMethod<TMethod, TParameters, TResult>
        where TParameters : notnull
        where TResult : notnull
    {
        var request = new RpcRequest<TParameters>
        {
            Method = TMethod.MethodName,
            Id = id ?? "0",
            Params = parameters
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request, typeof(RpcRequest<TParameters>), WalletRpcSerializerContext.Default),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

        var response = await http.PostAsync("/json_rpc", content, token);
        return await response.Content.ReadFromJsonAsync(typeof(RpcResponse<TResult>), WalletRpcSerializerContext.Default, token) as RpcResponse<TResult>
            ?? throw new Exception("Invalid RPC response.");
    }
}
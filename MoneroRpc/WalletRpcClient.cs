using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dosiero.MoneroRpc;

public partial class WalletRpcClient(HttpClient http)
{
    public JsonIgnoreCondition DefaultIgnoreCondition { get; private set; }

    private async Task<RpcResponse<TResult>> CallAsync<TMethod, TParameters, TResult>(TParameters parameters, string? id = null, CancellationToken token = default)
        where TMethod : IRpcMethod<TMethod, TParameters, TResult>
        where TParameters : notnull
        where TResult : notnull
    {
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
        };

        var request = new RpcRequest<TParameters>
        {
            Method = TMethod.MethodName,
            Id = id ?? "0",
            Params = parameters
        };
        var content = new StringContent(
            JsonSerializer.Serialize(request, options),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

        var response = await http.PostAsync("/json_rpc", content, token);
        return await response.Content.ReadFromJsonAsync<RpcResponse<TResult>>()
            ?? throw new Exception("Invalid RPC response.");
    }
}
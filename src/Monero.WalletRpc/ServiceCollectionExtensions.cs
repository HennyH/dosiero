using Microsoft.Extensions.DependencyInjection;

using System.Net;

namespace Monero.WalletRpc;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWalletRpc(this IServiceCollection services, Func<IServiceProvider, WalletRpcOptions> configureOptions)
    {
        services
            .AddHttpClient<IWalletRpcClient, WalletRpcClient>()
            .ConfigureHttpClient((services, client) =>
            {
                var options = configureOptions(services);
                client.BaseAddress = options.Uri;
            })
            .ConfigurePrimaryHttpMessageHandler(services =>
            {
                var options = configureOptions(services);
                var handler = new HttpClientHandler();

                handler.AcceptSelfSignedCerts(options.AcceptSelfSignedCerts);

                if (options is { Username: { } username, Password: var password })
                {
                    var credentials = new CredentialCache
                    {
                        { options.Uri, "Digest", new NetworkCredential(username, password) },
                    };

                    handler.PreAuthenticate = true;
                    handler.Credentials = credentials;
                }

                return handler;
            });

        return services;
    }
}

file static class HttpClientHandlerExtensions
{
    public static void AcceptSelfSignedCerts(this HttpClientHandler handler, bool acceptSelfSignedCerts = true)
    {
        if (acceptSelfSignedCerts)
        {
            handler.ServerCertificateCustomValidationCallback += static (_, _, _, _) => true;
        }
    }
}

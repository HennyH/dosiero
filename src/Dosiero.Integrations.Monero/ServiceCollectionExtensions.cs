using Dosiero.Abstractions.Payments;

using Microsoft.Extensions.Options;

using Monero.WalletRpc;

namespace Dosiero.Integrations.Monero;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMoneroPayments(this IServiceCollection services, string configSectionPath = MoneroPaymentOptions.MoneroPayment)
    {
        services
            .AddOptionsWithValidateOnStart<MoneroPaymentOptions>()
            .BindConfiguration(configSectionPath);
        services
            .AddWalletRpc(services =>
            {
                var options = services.GetRequiredService<IOptions<MoneroPaymentOptions>>().Value;
                return options.ToWalletRpcOptions();
            });
        services.AddPaymentIntegration<MoneroPaymentIntegration, SignedMoneroPayment>();
        return services;
    }
}

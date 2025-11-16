using Microsoft.Extensions.DependencyInjection;

namespace Dosiero.Configuration;

public interface IPaymentMethodCommand<in TOptions> : IConfigurationCommand<TOptions>
{
    public abstract static string Ticker { get; }

    public abstract static IServiceCollection AddPaymentMethod(IServiceCollection services);
}

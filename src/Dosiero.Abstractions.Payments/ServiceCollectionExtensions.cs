using Microsoft.Extensions.DependencyInjection;

using System.Diagnostics.CodeAnalysis;

namespace Dosiero.Abstractions.Payments;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentIntegration<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TIntegration, TPayment>(this IServiceCollection services)
        where TIntegration : class, IPaymentIntegration<TIntegration, TPayment>
        where TPayment : IPayment
    {
        services.AddKeyedScoped<IPaymentIntegration, TIntegration>(TIntegration.Scheme);
        return services;
    }
}

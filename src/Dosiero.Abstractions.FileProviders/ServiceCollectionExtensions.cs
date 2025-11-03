using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System.Diagnostics.CodeAnalysis;

namespace Dosiero.Abstractions.FileProviders;

file sealed class IntegratedFileProvider<TFileProvider>(TFileProvider provider) : IIntegratedFileProvider<TFileProvider>
    where TFileProvider : IFileProvider
{
    public TFileProvider Provider => provider;
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFileProvider>(this IServiceCollection services)
        where TFileProvider : class, IFileProvider
    {
        services.TryAddScoped<IFileProvider, DelegatingFileProvider>();
        services.AddScoped<TFileProvider>();
        services.AddScoped<IIntegratedFileProvider<IFileProvider>, IntegratedFileProvider<TFileProvider>>();
        return services;
    }
}

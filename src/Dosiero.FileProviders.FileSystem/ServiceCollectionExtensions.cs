using Dosiero.Abstractions.FileProviders;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Options;

namespace Dosiero.FileProviders.FileSystem;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFsFileProvider(this IServiceCollection services, string configSectionPath = FsFileProviderOptions.FsFileProvider)
    {
        services
            .AddOptionsWithValidateOnStart<FsFileProviderOptions>()
            .BindConfiguration(configSectionPath);
        services
            .AddSingleton(services =>
            {
                var options = services.GetRequiredService<IOptions<FsFileProviderOptions>>().Value;
                return new PhysicalFileProvider(options.Path, ExclusionFilters.Hidden | ExclusionFilters.System | ExclusionFilters.DotPrefixed);
            });
        services.AddFileProvider<FsFileProvider>();
        return services;
    }
}

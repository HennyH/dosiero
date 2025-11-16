using Microsoft.Extensions.DependencyInjection;

namespace Dosiero.Configuration;

public interface IFileProviderCommand<TOptions> : IConfigurationCommand<TOptions>
{
    public abstract static string Scheme { get; }

    public abstract static IServiceCollection AddFileProvider(IServiceCollection services);
}

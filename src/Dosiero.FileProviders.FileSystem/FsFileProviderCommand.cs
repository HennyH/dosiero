using Dosiero.Abstractions.FileProviders;
using Dosiero.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Options;

using System.CommandLine;
using System.CommandLine.Parsing;

namespace Dosiero.FileProviders.FileSystem;

public sealed class FsFileProviderCommand : IFileProviderCommand<FsFileProviderOptions>
{
    public static string Scheme => "fs";

    public static string Description => "Add and configure the file system file provider.";

    public static Argument<string[]> Folders { get; } = Argument<string[]>.Create(() =>
    {
        var folders = new Argument<string[]>("folder")
        {
            Description = "The folder to provide files from.",
            Arity = ArgumentArity.OneOrMore,
            CustomParser = ValidateFoldersArgument
        };
        folders.AcceptLegalFilePathsOnly();
        return folders;
    });

    public static void ConfigureCommand(Command command)
    {
        command.Arguments.Add(Folders);
    }

    private static string[] ValidateFoldersArgument(ArgumentResult arg)
    {
        var paths = new List<string>();

        foreach (var token in arg.Tokens)
        {
            var path = Path.ExpandUnixPath(token.Value);

            if (!Path.Exists(path))
            {
                arg.AddError($"The path '{path}' does not exist");
            }
            else if (!Directory.Exists(path))
            {
                arg.AddError($"The path '{path}' is not a folder");
            }

            paths.Add(path);
        }

        return [.. paths];
    }

    public static void BindConfiguration(FsFileProviderOptions options, ParseResult result)
    {
        var folders = result.GetRequiredValue(Folders);

        if (folders.Length > 1)
        {
            /* TODO: implement multi folder support */
            throw new NotImplementedException("Multi folder support has not been implemented yet.");
        }

        options.Path = folders[0];
    }

    public static IServiceCollection AddFileProvider(IServiceCollection services)
    {
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

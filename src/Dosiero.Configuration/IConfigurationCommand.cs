using System.CommandLine;

namespace Dosiero.Configuration;

public interface IConfigurationCommand<in TOptions>
{
    public abstract static string Description { get; }

    public abstract static void ConfigureCommand(Command command);

    public abstract static void BindConfiguration(TOptions options, ParseResult result);
}

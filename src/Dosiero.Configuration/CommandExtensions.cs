using System.CommandLine;

namespace Dosiero.Configuration;

public static class CommandExtensions
{
    extension(Command)
    {
        public static Command Create(Func<Command> factory) => factory();
    }

    extension(RootCommand)
    {
        public static RootCommand Create(Func<RootCommand> factory) => factory();
    }
}

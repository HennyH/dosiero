using System.CommandLine;

namespace Dosiero.Configuration;

public static class ArgumentExtensions
{
    extension<T>(Argument<T>)
    {
        public static Argument<T> Create(Func<Argument<T>> factory) => factory();
    }
}

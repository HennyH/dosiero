namespace Dosiero.UriUtility;

public sealed class UrlFormatProvider : IFormatProvider, ICustomFormatter
{
    public static readonly UrlFormatProvider Instance = new();

    public object? GetFormat(Type? formatType) =>
        formatType == typeof(ICustomFormatter) ? this : null;


    public string Format(string? format, object? arg, IFormatProvider? formatProvider)
    {
        if (arg is null)
        {
            return string.Empty;
        }

        if (arg is UriRaw raw)
        {
            var text = raw.Value?.ToString();

            return text ?? string.Empty;
        }

        if (arg is UriParam param)
        {
            var text = param.Value?.ToString();

            if (text is null)
            {
                return string.Empty;
            }

            return $"{param.Name}={Uri.EscapeDataString(text)}";
        }

        {
            var text = arg.ToString();

            if (text is null)
            {
                return string.Empty;
            }

            return string.IsNullOrWhiteSpace(format) ? Uri.EscapeDataString(text) : $"{format}={Uri.EscapeDataString(text)}";
        }
    }
}

public readonly record struct UriParam(string Name, object? Value);

public readonly record struct UriRaw(object? Value);

public static class UriExtensions
{
    extension(Uri)
    {
        public static Uri From(FormattableString formattable)
            => new(string.Format(UrlFormatProvider.Instance, formattable.Format, formattable.GetArguments()));

        public static UriParam Param(string name, object? value) => new(name, value);

        public static UriRaw Raw(object? value) => new(value);
    }
}

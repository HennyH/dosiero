namespace Dosiero;

internal sealed class UrlFormatProvider : IFormatProvider, ICustomFormatter
{
    public static UrlFormatProvider Instance = new();

    public object? GetFormat(Type? formatType) =>
        formatType == typeof(ICustomFormatter) ? this : null;


    public string Format(string? format, object? arg, IFormatProvider? formatProvider)
    {
        if (arg is null)
        {
            return string.Empty;
        }

        if (arg is UrlRaw raw)
        {
            var text = raw.Value?.ToString();

            return text ?? string.Empty;
        }

        if (arg is UrlParam param)
        {
            var text = param.Value?.ToString();

            if (text is null)
            {
                return string.Empty;
            }

            return $"{param.Name}={Uri.EscapeDataString(text)}";
        }

        if (arg is IFormattable formattable)
        {
            var text = formattable.ToString();

            if (text is null)
            {
                return string.Empty;
            }

            return format is null ? $"{format}={Uri.EscapeDataString(text)}" : Uri.EscapeDataString(text);
        }

        {
            var text = arg.ToString();

            if (text is null)
            {
                return string.Empty;
            }

            return Uri.EscapeDataString(text);
        }
    }
}

internal readonly record struct UrlParam(string Name, object? Value);

internal readonly record struct UrlRaw(object? Value);

internal static class UriExtensions
{
    extension(Uri)
    {
        public static Uri From(FormattableString formattable)
            => new(string.Format(UrlFormatProvider.Instance, formattable.Format, formattable.GetArguments()));
    }
}
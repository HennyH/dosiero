using Dosiero.Abstractions.Payments;

using Microsoft.AspNetCore.Html;

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Dosiero;

internal static partial class DosieroFileParser
{
    public static DosieroFileParseResult ParseFile(string fileName)
    {
        var file = new FileInfo(fileName);

        if (!file.Exists)
        {
            return new DosieroFileParseResult.FileNotFound(file.FullName);
        }

        if (file.Extension is not ".dosiero")
        {
            return new DosieroFileParseResult.WrongFileExtension(file.Extension);
        }

        using var stream = File.OpenRead(file.FullName);
        using var reader = new StreamReader(stream, encoding: Encoding.UTF8);

        var htmlBuilder = new StringBuilder();
        var pricings = new Dictionary<string, FilePrice>();

        var foundSettingsSectionOpener = false;
        while (reader.ReadLine() is { } line)
        {
            if (line is "---")
            {
                foundSettingsSectionOpener = true;
                break;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            /* there must be no settings section */
            htmlBuilder.AppendLine(line);
            break;
        }

        var foundClosingDelimiter = false;
        if (foundSettingsSectionOpener)
        {
            while (reader.ReadLine() is { } line)
            {
                if (line is "---")
                {
                    foundClosingDelimiter = true;
                    break;
                }

                var match = FileSettingsRegex().Match(line);

                if (!match.Success)
                {
                    return new DosieroFileParseResult.InvalidFile($"The pricing line '{line}' was not in the required format of '<glob>[ = <price>]'");
                }

                var globText = match.Groups["glob"].Value.Trim();

                if (globText.Contains(".."))
                {
                    return new DosieroFileParseResult.InvalidFile($"The glob pattern '{globText}' cannot contain '..'");
                }

                if (globText.StartsWith(Path.PathSeparator) || globText.StartsWith(@"/") || globText.Contains(@":\\"))
                {
                    return new DosieroFileParseResult.InvalidFile($@"The glob pattern '{globText}' must be a relative path and cannot start with '\', '/' or ':\\'");
                }

                if (!ValidGlobPatternRegex().IsMatch(globText))
                {
                    return new DosieroFileParseResult.InvalidFile($"The glob pattern '{globText}' contains invalid characters");
                }

                FilePrice price;
                if (match.Groups["price"].Success)
                {
                    var priceText = match.Groups["price"].Value.Trim();

                    if (!decimal.TryParse(priceText, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, default, out var parsedPrice))
                    {
                        return new DosieroFileParseResult.InvalidFile($"The pricing text '{priceText}' could not be intepreted as a number");
                    }

                    price = new FilePrice.Paid(parsedPrice);
                }
                else
                {
                    price = new FilePrice.Free();
                }

                pricings[globText] = price;
            }
        }

        if (foundSettingsSectionOpener && !foundClosingDelimiter)
        {
            return new DosieroFileParseResult.InvalidFile("The file did not finish a settings section with '---'");
        }

        IHtmlContent? html;
        {
            while (reader.ReadLine() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                htmlBuilder.AppendLine(line);
            }

            html = htmlBuilder is not [] ? new HtmlString(htmlBuilder.ToString()) : null;
        }

        var entry = new DosieroFile
        {
            FileName = fileName,
            GlobToPrice = pricings,
            ReadMeHtml = html
        };

        return new DosieroFileParseResult.Ok(entry);
    }

    [GeneratedRegex(@"^\s*(?<glob>[^=]+)(\s*=\s*(?<price>[\d\.,]+))?\s*$", RegexOptions.NonBacktracking | RegexOptions.Singleline)]
    private static partial Regex FileSettingsRegex();

    [GeneratedRegex(@"^[\w\-\.\/\*\?\[\]]+$", RegexOptions.NonBacktracking | RegexOptions.Singleline)]
    private static partial Regex ValidGlobPatternRegex();
}

internal abstract record DosieroFileParseResult
{
    public sealed record Ok(DosieroFile Index) : DosieroFileParseResult;

    public sealed record FileNotFound(string FileName) : DosieroFileParseResult;

    public sealed record WrongFileExtension(string FileExtension) : DosieroFileParseResult;

    public sealed record FileNotReadable(string FileExtension) : DosieroFileParseResult;

    public sealed record InvalidFile(string Reason) : DosieroFileParseResult;
}

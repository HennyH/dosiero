using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Dosiero;

public static partial class DosieroIndexFileParser
{
    public static DosieroIndexFileParseResult ParseFile(string rootPath, string fileName)
    {
        var file = new FileInfo(fileName);

        if (!file.Exists)
        {
            return new DosieroIndexFileParseResult.FileNotFound(file.FullName);
        }

        if (file.Extension is not ".dosiero")
        {
            return new DosieroIndexFileParseResult.WrongFileExtension(file.Extension);
        }

        using var stream = File.OpenRead(file.FullName);
        using var reader = new StreamReader(stream, encoding: Encoding.UTF8);
        var entries = new List<DosieroIndexEntry>();

        while (true)
        {
            var result = ReadEntry(reader, shouldLookForOpeningDelimiter: entries is []);
            if (result is DosieroIndexEntryParseResult.EndOfFile)
            {
                break;
            }
            else if (result is DosieroIndexEntryParseResult.Ok { Entry: var entry })
            {
                entries.Add(entry);
            }
            else
            {
                return new DosieroIndexFileParseResult.InvalidFile($"Failed to parse entry: {result}");
            }
        }

        /* TODO: handle the null and empty path cases here better */
        var subPath = Path.GetDirectoryName(Path.GetRelativePath(relativeTo: rootPath, fileName));
        /* TODO: consider if having the / hardcoded is the best approach... perhaps it is */
        subPath = string.IsNullOrWhiteSpace(subPath) ? "/" : subPath;
        var node = new DosieroIndex(fileName, subPath, [..entries], file.LastWriteTimeUtc);
        return new DosieroIndexFileParseResult.Ok(node);
    }

    private static DosieroIndexEntryParseResult ReadEntry(StreamReader reader, bool shouldLookForOpeningDelimiter)
    {
        if (shouldLookForOpeningDelimiter)
        {
            while (reader.ReadLine() is { } line)
            {
                if (line is "---")
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                return new DosieroIndexEntryParseResult.InvalidEntry("The file did not start a files listing section with '---' as the first line");
            }
        }

        if (reader.EndOfStream)
        {
            return new DosieroIndexEntryParseResult.EndOfFile();
        }

        var settings = new List<FileSettings>();
        var foundClosingDelimiter = false;
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
                return new DosieroIndexEntryParseResult.InvalidEntry($"The pricing line '{line}' was not in the required format of '<glob>[ = <price>]'");
            }

            var globText = match.Groups["glob"].Value.Trim();

            if (globText.Contains(".."))
            {
                return new DosieroIndexEntryParseResult.InvalidEntry($"The glob pattern '{globText}' cannot contain '..'");
            }

            if (globText.StartsWith(Path.PathSeparator) || globText.StartsWith(@"/") || globText.Contains(@":\\"))
            {
                return new DosieroIndexEntryParseResult.InvalidEntry($@"The glob pattern '{globText}' must be a relative path and cannot start with '\', '/' or ':\\'");
            }

            if (!ValidGlobPatternRegex().IsMatch(globText))
            {
                return new DosieroIndexEntryParseResult.InvalidEntry($"The glob pattern '{globText}' contains invalid characters");
            }

            decimal? price;
            if (match.Groups["price"].Success)
            {
                var priceText = match.Groups["price"].Value.Trim();

                if (!decimal.TryParse(priceText, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, default, out var parsedPrice))
                {
                    return new DosieroIndexEntryParseResult.InvalidEntry($"The pricing text '{priceText}' could not be intepreted as a number");
                }

                price = parsedPrice;
            }
            else
            {
                price = null;
            }

            settings.Add(new FileSettings(globText, price));
        }

        if (!foundClosingDelimiter)
        {
            return new DosieroIndexEntryParseResult.InvalidEntry("The entry did not finish a files listing section with '---'");
        }

        if (settings is [])
        {
            return new DosieroIndexEntryParseResult.InvalidEntry("The entry did not reference any files");
        }

        string? html;
        {
            var builder = new StringBuilder();

            while (reader.ReadLine() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line is "---")
                {
                    break;
                }

                builder.AppendLine(line);
            }

            html = builder is not [] ? builder.ToString() : null;
        }

        var entry = new DosieroIndexEntry([.. settings], html);
        return new DosieroIndexEntryParseResult.Ok(entry);
    }

    [GeneratedRegex(@"^\s*(?<glob>[^=]+)(\s*=\s*(?<price>[\d\.,]+))?\s*$", RegexOptions.NonBacktracking | RegexOptions.Singleline)]
    private static partial Regex FileSettingsRegex();

    [GeneratedRegex(@"^[\w\-\.\/\*\?\[\]]+$", RegexOptions.NonBacktracking | RegexOptions.Singleline)]
    private static partial Regex ValidGlobPatternRegex();
}

public abstract record DosieroIndexFileParseResult
{
    public sealed record Ok(DosieroIndex Index) : DosieroIndexFileParseResult;

    public sealed record FileNotFound(string FileName) : DosieroIndexFileParseResult;

    public sealed record WrongFileExtension(string FileExtension) : DosieroIndexFileParseResult;

    public sealed record FileNotReadable(string FileExtension) : DosieroIndexFileParseResult;

    public sealed record InvalidFile(string Reason) : DosieroIndexFileParseResult;

    public sealed record EndOfFile : DosieroIndexFileParseResult;
}

internal abstract record DosieroIndexEntryParseResult
{
    public sealed record Ok(DosieroIndexEntry Entry) : DosieroIndexEntryParseResult;

    public sealed record InvalidEntry(string Reason) : DosieroIndexEntryParseResult;

    public sealed record EndOfFile : DosieroIndexEntryParseResult;
}
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileSystemGlobbing;

using System.Diagnostics.CodeAnalysis;

namespace Dosiero;

public class DosieroIndexesBasedFilePricer(IDosieroIndexesWatcher dosieroFileWatcher) : IFilePricer
{
    public FilePrice GetFilePrice(IFileInfo file)
    {
        if (file.IsDirectory)
        {
            throw new NotImplementedException($"Support for pricing files has not been implemented in {nameof(DosieroIndexesBasedFilePricer)}");
        }

        if (string.IsNullOrWhiteSpace(file.PhysicalPath))
        {
            throw new ArgumentException($"Cannot price file {file} which has no physical path");
        }

        foreach (var index in dosieroFileWatcher.Indexes.OrderByDescending(f => f.FileName.Length))
        {
            var directory = Path.GetDirectoryName(index.FileName)!;

            if (!TryGetRelativePath(directory, file.PhysicalPath, out var relativePath))
            {
                continue;
            }

            /* last entry trumps previous */
            foreach (var entry in index.Entries.Reverse())
            {
                /* last setting trumps previous */
                foreach (var setting in entry.FileSettings.Reverse())
                {
                    if (setting.Price is null or 0)
                    {
                        continue;
                    }

                    if (Matcher.IsMatch(setting.Glob, relativePath))
                    {
                        return new FilePrice.Paid(setting.Price.Value);
                    }
                }
            }
        }

        return new FilePrice.Free();
    }

    private static bool TryGetRelativePath(string relativeTo, string path, [NotNullWhen(true)] out string? relativePath)
    {
        relativePath = Path.GetRelativePath(relativeTo, path);
        /* the `Path.GetRelativePath` will return the value of `path` if it is not a path underneath `relativeTo`, hence we do this comparison */
        relativePath = relativePath != path ? relativePath : null;
        return relativePath is not null;
    }
}


file static class MatcherExtensions
{
    extension(Matcher)
    {
        public static bool IsMatch(string glob, string fileName)
        {
            var matcher = new Matcher();
            matcher.AddInclude(glob);
            return matcher.Match(fileName).HasMatches;
        }
    }
}
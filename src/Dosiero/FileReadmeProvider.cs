using Dosiero.Abstractions.FileProviders;

using Microsoft.AspNetCore.Html;

using System.Text.RegularExpressions;

namespace Dosiero;

internal partial class FileReadmeProvider(ILogger<FileReadmeProvider> logger) : IFileReadmeProvider
{
    private readonly List<ReadmeAssignemnt> _assignments = [];

    public void AddFileReadme(LikeString pattern, string path)
    {
        _assignments.Add(new ReadmeAssignemnt(pattern, path));
    }

    public async ValueTask<IHtmlContent?> GetFileReadmeAsync(IFileInfo file, CancellationToken token = default)
    {
        foreach (var (pattern, path) in _assignments)
        {
            if (pattern.Match(file.Uri.AbsolutePath) is { IsMatch: true, Captures: var captures })
            {
                try
                {
                    string ReplaceWithCapture(Match match)
                    {
                        var index = int.Parse(match.Groups["index"].Value);
                        return captures[index];
                    }

                    var resolved = SubstitionRegex().Replace(path, ReplaceWithCapture);

                    if (Path.Exists(resolved))
                    {
                        var html = await File.ReadAllTextAsync(resolved);
                        return new HtmlString(html);
                    }
                }
                catch (Exception error)
                {
                    logger.LogError(error, "Failed to resolve path '{Path}' from captures {@Captures}", path, captures);
                }
            }
        }

        return default;
    }

    private sealed record ReadmeAssignemnt(LikeString pattern, string path);

    [GeneratedRegex(@"(?<!\\)%(?<index>\d+)")]
    private static partial Regex SubstitionRegex();
}

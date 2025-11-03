using Dosiero.Abstractions.FileProviders;

using System.Text.RegularExpressions;

using PhysicalFileProvider = Microsoft.Extensions.FileProviders.PhysicalFileProvider;

namespace Dosiero.FileProviders.FileSystem;

internal sealed partial class FsFileProvider(PhysicalFileProvider fs) : IFileProvider
{
    public Uri Root => FsRootInfo.Instance.Uri;

    public ValueTask<IFileInfo[]> GetDirectoryContentsAsync(Uri uri, CancellationToken token = default)
    {
        var files = new List<IFileInfo>();

        foreach (var info in fs.GetDirectoryContents(uri.AbsolutePath))
        {
            if (info.Name.EndsWith(".dosiero"))
            {
                continue;
            }

            files.Add(new FsFileInfo(
                uri: new Uri($"fs:{uri.AbsolutePath.TrimEnd('/')}{(uri == Root ? "" : "/")}{info.Name}{(info.IsDirectory ? "/" : "")}"),
                info: info));
        }

        return ValueTask.FromResult<IFileInfo[]>([.. files]);
    }

    public ValueTask<IFileInfo> GetFileInfoAsync(Uri uri, CancellationToken token = default)
    {
        if (uri == Root)
        {
            return ValueTask.FromResult<IFileInfo>(FsRootInfo.Instance);
        }

        if (uri.AbsolutePath.EndsWith('/'))
        {
            var name = DirectoryNameRegex().Match(uri.AbsolutePath) is { Success: true } match
                ? match.Groups["name"].Value
                : string.Empty;
            var info = fs.GetDirectoryContents(uri.AbsolutePath);
            return ValueTask.FromResult<IFileInfo>(new FsDirectoryInfo(
                Uri: uri,
                Exists: info.Exists,
                Name: name));
        }

        return ValueTask.FromResult<IFileInfo>(new FsFileInfo(
            uri: uri,
            info: fs.GetFileInfo(uri.AbsolutePath)));
    }

    public ValueTask<IFileInfo> GetParentDirectoryInfoAsync(Uri uri, CancellationToken token = default)
    {
        if (uri == Root)
        {
            return ValueTask.FromResult<IFileInfo>(FsRootInfo.Instance);
        }

        if (ParentDirectoryPathRegex().Match(uri.AbsolutePath) is not { Success: true } match)
        {
            return ValueTask.FromResult<IFileInfo>(FsRootInfo.Instance);
        }

        var directory = match.Groups["path"].Value;
        return GetFileInfoAsync(new Uri($"fs:{directory}"), token);
    }

    public ValueTask<Stream> OpenReadStreamAsync(Uri uri, CancellationToken token = default)
    {
        var info = fs.GetFileInfo(uri.AbsolutePath);
        return ValueTask.FromResult(info.CreateReadStream());
    }

    [GeneratedRegex(@"(^|/)(?<name>[^/]+)/$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.RightToLeft)]
    private static partial Regex DirectoryNameRegex();

    [GeneratedRegex(@"^(?<path>.+/)([^/]+/?)$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex ParentDirectoryPathRegex();
}

file sealed class FsFileInfo(Uri uri, Microsoft.Extensions.FileProviders.IFileInfo info) : IFileInfo
{
    public Uri Uri => uri;

    public bool Exists => info.Exists;

    public string Name => info.Name;

    public bool IsDirectory => info.IsDirectory;
}

file sealed record FsRootInfo : IFileInfo
{
    private FsRootInfo() { }

    public Uri Uri { get; } = new Uri("fs:");

    public bool Exists { get; } = true;

    public string Name { get; } = string.Empty;

    public bool IsDirectory { get; } = true;

    public static readonly FsRootInfo Instance = new();
}

file sealed record FsDirectoryInfo(Uri Uri, bool Exists, string Name) : IFileInfo
{
    public bool IsDirectory => true;
}
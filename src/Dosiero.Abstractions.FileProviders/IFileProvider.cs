namespace Dosiero.Abstractions.FileProviders;

public interface IFileProvider
{
    public Uri Root { get; }

    public virtual Uri GetRoot(Uri uri) => Root;

    public ValueTask<IFileInfo[]> GetDirectoryContentsAsync(Uri uri, CancellationToken token);

    public ValueTask<IFileInfo> GetFileInfoAsync(Uri uri, CancellationToken token);

    public ValueTask<IFileInfo> GetParentDirectoryInfoAsync(Uri uri, CancellationToken token);

    public ValueTask<Stream> OpenReadStreamAsync(Uri uri, CancellationToken token);
}

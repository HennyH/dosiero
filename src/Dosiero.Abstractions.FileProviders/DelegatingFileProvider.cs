namespace Dosiero.Abstractions.FileProviders;

internal class DelegatingFileProvider(IEnumerable<IIntegratedFileProvider<IFileProvider>> providers) : IFileProvider
{
    internal static readonly Uri Root = new("dosiero:");

    private readonly Dictionary<string, IFileProvider> _schemeToProvider = providers
        .ToDictionary(p => p.Provider.Root.Scheme, p => p.Provider, StringComparer.OrdinalIgnoreCase);

    Uri IFileProvider.Root => Root;

    public Uri GetRoot(Uri uri)
    {
        if (uri == Root)
        {
            return Root;
        }

        return GetProviderForUriOrThrow(uri).GetRoot(uri);
    }

    public async ValueTask<IFileInfo[]> GetDirectoryContentsAsync(Uri uri, CancellationToken token)
    {
        if (uri == Root)
        {
            var contents = new List<IFileInfo>();

            foreach (var provider in _schemeToProvider.Values)
            {
                contents.AddRange(await provider.GetDirectoryContentsAsync(provider.Root, token));
            }

            return [.. contents];
        }

        return await GetProviderForUriOrThrow(uri).GetDirectoryContentsAsync(uri, token);
    }

    public async ValueTask<IFileInfo> GetParentDirectoryInfoAsync(Uri uri, CancellationToken token)
    {
        if (uri == Root)
        {
            return RootFileInfo.Instance;
        }

        return await GetProviderForUriOrThrow(uri).GetParentDirectoryInfoAsync(uri, token);
    }

    public async ValueTask<IFileInfo> GetFileInfoAsync(Uri uri, CancellationToken token)
    {
        if (uri == Root)
        {
            return RootFileInfo.Instance;
        }

        return await GetProviderForUriOrThrow(uri).GetFileInfoAsync(uri, token);
    }

    public async ValueTask<Stream> OpenReadStreamAsync(Uri uri, CancellationToken token)
    {
        if (uri == Root)
        {
            throw new NotImplementedException();
        }

        return await GetProviderForUriOrThrow(uri).OpenReadStreamAsync(uri, token);
    }

    private IFileProvider GetProviderForUriOrThrow(Uri uri)
    {
        if (!_schemeToProvider.TryGetValue(uri.Scheme, out var provider))
        {
            throw new NotImplementedException($"No file provider for scheme '{uri.Scheme}' has been configured.");
        }

        return provider;
    }
}

file sealed class RootFileInfo : IFileInfo
{
    public Uri Uri => DelegatingFileProvider.Root;

    public bool Exists => true;

    public string Name => "~";

    public bool IsDirectory => true;

    public static readonly RootFileInfo Instance = new();
}
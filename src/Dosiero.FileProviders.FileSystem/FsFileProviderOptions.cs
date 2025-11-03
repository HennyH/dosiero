namespace Dosiero.FileProviders.FileSystem;

public sealed class FsFileProviderOptions
{
    public const string FsFileProvider = nameof(FsFileProvider);

    public required string Path { get; set; }
}

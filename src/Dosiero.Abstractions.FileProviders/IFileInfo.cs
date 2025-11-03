namespace Dosiero.Abstractions.FileProviders;

public interface IFileInfo
{
    public Uri Uri { get; }

    public bool Exists { get; }

    public string Name { get; }

    public bool IsDirectory { get; }
}

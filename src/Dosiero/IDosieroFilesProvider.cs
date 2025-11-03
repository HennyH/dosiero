namespace Dosiero;

public interface IDosieroFilesProvider
{
    public IEnumerable<IDosieroFile> Files { get; }
}
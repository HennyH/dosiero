namespace Dosiero;

public interface IDosieroIndexesProvider
{
    public IEnumerable<DosieroIndex> Indexes { get; }
}
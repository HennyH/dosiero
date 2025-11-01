namespace Dosiero;

public interface IDosieroIndexesWatcher : IDosieroIndexesProvider
{
    public Task WatchAsync(CancellationToken token = default);
}

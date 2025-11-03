namespace Dosiero;

public interface IDosieroFilesWatcher : IDosieroFilesProvider
{
    public Task WatchAsync(CancellationToken token = default);
}

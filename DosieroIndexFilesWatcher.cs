using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Options;

using System.Collections.Immutable;

namespace Dosiero;

internal sealed class DosieroIndexFilesWatcher(IOptions<DosieroOptions> options, ILogger<DosieroIndexFilesWatcher> logger) : IDosieroIndexesWatcher
{
    private ImmutableArray<DosieroIndex> indexes = [];

    public IEnumerable<DosieroIndex> Indexes => indexes;

    public async Task WatchAsync(CancellationToken token = default)
    {
        var matcher = new Matcher();
        matcher.AddInclude(".dosiero");

        foreach (var file in matcher.GetResultsInFullPath(options.Value.Folder))
        {
            indexes = indexes.Add(LoadFile(file));
        }

        using var watcher = new FileSystemWatcherEventChannel(
            path: options.Value.Folder,
            filter: ".dosiero",
            includeSubdirectories: true);

        watcher.Initialize();

        await foreach (var @event in watcher.FileSystemEvents.ReadAllAsync(token))
        {
            var next = indexes;

            next = next.RemoveAll(i => i.FileName == @event.FullPath);

            if (@event is RenamedEventArgs renamed)
            {
                next = next.RemoveAll(i => i.FileName == renamed.OldFullPath);
            }

            if (@event.ChangeType is not WatcherChangeTypes.Deleted)
            {
                next = next.Add(LoadFile(@event.FullPath));
            }

            indexes = next;
        }
    }

    private DosieroIndex LoadFile(string fileName)
    {
        logger.LogInformation("Loaded .dosiero file {FileName}", fileName);

        return DosieroIndexFileParser.ParseFile(options.Value.Folder, fileName) switch
        {
            DosieroIndexFileParseResult.Ok ok => ok.Index,
            var result => throw new Exception($"Failed to parse .dosiero file: {result}")
        };
    }
}

using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Options;

using System.Collections.Immutable;

namespace Dosiero;

internal sealed class DosieroFilesWatcher(ILogger<DosieroFilesWatcher> logger, IOptions<DosieroOptions> options) : IDosieroFilesWatcher
{
    private ImmutableArray<DosieroFile> files = [];

    public IEnumerable<IDosieroFile> Files => files;

    public async Task WatchAsync(CancellationToken token = default)
    {
        var matcher = new Matcher();
        matcher.AddInclude("*.dosiero");

        foreach (var file in matcher.GetResultsInFullPath(options.Value.ConfigFolder))
        {
            files = files.Add(LoadFile(file));
        }

        using var watcher = new FileSystemWatcherEventChannel(
            path: options.Value.ConfigFolder,
            filter: "*.dosiero",
            includeSubdirectories: true);

        watcher.Initialize();

        await foreach (var @event in watcher.FileSystemEvents.ReadAllAsync(token))
        {
            var next = files;

            next = next.RemoveAll(i => i.FileName == @event.FullPath);

            if (@event is RenamedEventArgs renamed)
            {
                next = next.RemoveAll(i => i.FileName == renamed.OldFullPath);
            }

            if (@event.ChangeType is not WatcherChangeTypes.Deleted)
            {
                next = next.Add(LoadFile(@event.FullPath));
            }

            files = next;
        }
    }

    private DosieroFile LoadFile(string fileName)
    {
        logger.LogInformation("Loaded .dosiero file {FileName}", fileName);

        return DosieroFileParser.ParseFile(fileName) switch
        {
            DosieroFileParseResult.Ok ok => ok.Index,
            var result => throw new Exception($"Failed to parse .dosiero file: {result}")
        };
    }
}

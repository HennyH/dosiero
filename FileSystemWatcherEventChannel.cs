using System.Threading.Channels;

namespace Dosiero;

internal sealed class FileSystemWatcherEventChannel(string path, string filter = "*.*", bool includeSubdirectories = false) : IDisposable
{
    private FileSystemWatcher? watcher;
    private readonly Channel<FileSystemEventArgs> channel = Channel.CreateUnbounded<FileSystemEventArgs>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    public ChannelReader<FileSystemEventArgs> FileSystemEvents => channel.Reader;

    public void Initialize()
    {
        watcher = new FileSystemWatcher(path, filter)
        {
            IncludeSubdirectories = includeSubdirectories,
            EnableRaisingEvents = true
        };

        watcher.Created += HandleFileEvent;
        watcher.Changed += HandleFileEvent;
        watcher.Deleted += HandleFileEvent;
        watcher.Renamed += HandleFileEvent;
    }

    private void HandleFileEvent(object? sender, FileSystemEventArgs args)
    {
        channel.Writer.TryWrite(args);
    }

    public void Dispose()
    {
        watcher?.Dispose();
    }
}
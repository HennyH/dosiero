namespace Dosiero;

public sealed record DosieroIndex(string FileName, string SubPath, DosieroIndexEntry[] Entries, DateTime LastWriteTimeUtc);

public sealed record DosieroIndexEntry(FileSettings[] FileSettings, string? Html);

public sealed record FileSettings(string Glob, decimal? Price);
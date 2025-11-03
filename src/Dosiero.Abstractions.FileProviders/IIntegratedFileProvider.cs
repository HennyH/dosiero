namespace Dosiero.Abstractions.FileProviders;

internal interface IIntegratedFileProvider<out TFileProvider>
    where TFileProvider : IFileProvider
{
    public TFileProvider Provider { get; }
}

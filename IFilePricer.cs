using Microsoft.Extensions.FileProviders;

namespace Dosiero;

public interface IFilePricer
{
    public FilePrice GetFilePrice(IFileInfo fileInfo);
}

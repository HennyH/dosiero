using Dosiero.Abstractions.FileProviders;
using Dosiero.Abstractions.Payments;

namespace Dosiero;

public interface IFilePricer
{
    public FilePrice GetPrice(IFileInfo file);
}
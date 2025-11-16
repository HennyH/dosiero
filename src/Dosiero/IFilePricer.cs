using Dosiero.Abstractions.FileProviders;
using Dosiero.Abstractions.Payments;

namespace Dosiero;

internal interface IFilePricer
{
    public void SetPrice(LikeString pattern, FilePrice price);

    public FilePrice GetPrice(IFileInfo file);
}
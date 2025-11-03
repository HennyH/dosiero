using Dosiero.Abstractions.FileProviders;
using Dosiero.Abstractions.Payments;

using Microsoft.Extensions.FileSystemGlobbing;

namespace Dosiero;

internal class DosieroFileBasedPricer(IDosieroFilesProvider dosieroFilesProvider) : IFilePricer
{
    public FilePrice GetPrice(IFileInfo file)
    {
        foreach (var dosieroFile in dosieroFilesProvider.Files)
        {
            foreach (var (glob, price) in dosieroFile.GlobToPrice)
            {
                var matcher = new Matcher();
                matcher.AddInclude(glob);

                if (matcher.Match(file.Uri.AbsolutePath).HasMatches)
                {
                    return price;
                }
            }
        }

        return new FilePrice.Free();
    }
}

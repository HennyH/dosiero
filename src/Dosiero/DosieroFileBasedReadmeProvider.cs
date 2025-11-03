using Dosiero.Abstractions.FileProviders;

using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Dosiero;

internal class DosieroFileBasedReadmeProvider(IDosieroFilesProvider dosieroFilesProvider) : IFileReadMeProvider
{
    public IHtmlContent? GetFileReadMe(IFileInfo file)
    {
        foreach (var dosieroFile in dosieroFilesProvider.Files)
        {
            if (dosieroFile.ReadMeHtml is null)
            {
                continue;
            }

            foreach (var (glob, _) in dosieroFile.GlobToPrice)
            {
                var matcher = new Matcher();
                matcher.AddInclude(glob);

                if (matcher.Match(file.Uri.AbsolutePath).HasMatches)
                {
                    return dosieroFile.ReadMeHtml;
                }
            }
        }

        return default;
    }
}

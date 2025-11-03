using Dosiero.Abstractions.FileProviders;

using Microsoft.AspNetCore.Html;

namespace Dosiero;

public interface IFileReadMeProvider
{
    public IHtmlContent? GetFileReadMe(IFileInfo file);
}

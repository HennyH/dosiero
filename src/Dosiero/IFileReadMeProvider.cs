using Dosiero.Abstractions.FileProviders;

using Microsoft.AspNetCore.Html;

namespace Dosiero;

internal interface IFileReadmeProvider
{
    public void AddFileReadme(LikeString pattern, string path);

    public ValueTask<IHtmlContent?> GetFileReadmeAsync(IFileInfo file, CancellationToken token = default);
}

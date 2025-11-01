using Dosiero.Components;
using Dosiero.Components.Pages;

using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace Dosiero;

public class BlazorDirectoryFormatter : IDirectoryFormatter
{
    public async Task GenerateContentAsync(HttpContext context, IEnumerable<IFileInfo> contents)
    {
        var result = TypedResults.Blazor<App>(NavigationInit.ForHttpContext(context), [RenderParameter.For(nameof(Files.Entires), contents)]);
        await result.ExecuteAsync(context);
    }
}

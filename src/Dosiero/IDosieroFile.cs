using Dosiero.Abstractions.Payments;

using Microsoft.AspNetCore.Html;

namespace Dosiero;

public interface IDosieroFile
{
    public Dictionary<string, FilePrice> GlobToPrice { get; }

    public IHtmlContent? ReadMeHtml { get; }
}

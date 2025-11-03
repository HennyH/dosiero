using Dosiero.Abstractions.Payments;

using Microsoft.AspNetCore.Html;

namespace Dosiero;

internal sealed record DosieroFile : IDosieroFile
{
    public required string FileName { get; init; }

    public Dictionary<string, FilePrice> GlobToPrice { get; set; } = [];

    public IHtmlContent? ReadMeHtml { get; init; }
}

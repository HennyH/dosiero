using Dosiero.Slices.Pages;

using Microsoft.AspNetCore.Mvc;

namespace Dosiero.Endpoints;

public static class BrowseEndpoints
{
    private static IResult GetBrowsePage([FromQuery(Name = "file")] Uri? file)
        => TypedResults.Extensions.RazorSlice<Browse, Uri?>(file);

    public static void MapBrowseEndpoints(this WebApplication app)
    {
        app.MapGet("/", GetBrowsePage);
    }
}


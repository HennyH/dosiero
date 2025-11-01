using Dosiero.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

namespace Dosiero;

internal sealed class BlazorResult<TComponent>(NavigationInit navigationInit, params IRenderParameter[] parameters) : IResult
    where TComponent : IComponent
{
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        /* TODO: Can this write be streamed and async? */
        await using var scope = httpContext.RequestServices.CreateAsyncScope();
        var html = await BlazorRenderer.RenderToStringAsync<TComponent>(scope, navigationInit, parameters.ToParameterView());
        await using var writer = new StreamWriter(httpContext.Response.Body);
        await writer.WriteAsync(html);
    }
}

internal static class BlazorRenderer
{
    public static async Task<string> RenderToStringAsync<TComponent>(AsyncServiceScope scope, NavigationInit navigationInit, ParameterView parameters)
        where TComponent : IComponent
    {
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

        switch (navigationInit)
        {
            case NoNavigation:
                break;
            case HttpNavigation http:
                http.InitializeNavigationManager(scope);
                break;
            default:
                throw new NotImplementedException($"Unsupported navigation init {navigationInit.GetType()}.");
        }

        await using var renderer = new HtmlRenderer(scope.ServiceProvider, loggerFactory);
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var rendered = await renderer.RenderComponentAsync<TComponent>(parameters);
            return rendered.ToHtmlString();
        });
    }
}

public abstract record NavigationInit
{
    public readonly static NoNavigation None = NoNavigation.Instance;

    public static HttpNavigation ForHttpContext(HttpContext context, string? path = null)
        => new(context, path);
}

public sealed record NoNavigation : NavigationInit
{
    public readonly static NoNavigation Instance = new();
}

public sealed record HttpNavigation(HttpContext HttpContext, string? Path) : NavigationInit
{
    public void InitializeNavigationManager(IServiceScope scope)
    {
        if (scope.ServiceProvider.GetRequiredService<NavigationManager>() is not IHostEnvironmentNavigationManager navigationManager)
        {
            throw new NotImplementedException($"Navigation init is only supported for {typeof(IHostEnvironmentNavigationManager)}.");
        }

        navigationManager.Initialize(
            baseUri: $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/",
            uri: $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{Path ?? HttpContext.Request.Path}");
    }
}

internal static class TypedResultExtensions
{
    extension(TypedResults)
    {
        public static BlazorResult<TComponent> Blazor<TComponent>(NavigationInit navigationInit, IRenderParameter[] parameters)
            where TComponent : IComponent
                => new(navigationInit, parameters);
    }
}
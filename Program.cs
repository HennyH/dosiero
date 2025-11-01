using Dosiero;
using Dosiero.Components;
using Dosiero.MoneroRpc;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Options;

using System.Buffers.Text;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddRazorComponents();
builder.Services.AddSingleton<BlazorDirectoryFormatter>();
builder.Services.AddSingleton<IFileProvider>(services =>
{
    var options = services.GetRequiredService<IOptions<DosieroOptions>>();
    return new PhysicalFileProvider(options.Value.Folder, ExclusionFilters.Hidden | ExclusionFilters.System | ExclusionFilters.DotPrefixed);
});
builder.Services
    .AddOptions<DosieroOptions>()
    .BindConfiguration(DosieroOptions.Dosiero);
builder.Services
    .AddOptions<DirectoryBrowserOptions>()
    .Configure<IFileProvider, BlazorDirectoryFormatter>((options, fileProvider, formatter) =>
    {
        options.RedirectToAppendTrailingSlash = true;
        options.RequestPath = "/files";
        options.Formatter = formatter;
        options.FileProvider = fileProvider;
    });
builder.Services.AddSingleton<DosieroIndexFilesWatcher>();
builder.Services.AddSingleton<IDosieroIndexesWatcher>(sp => sp.GetRequiredService<DosieroIndexFilesWatcher>());
builder.Services.AddSingleton<IDosieroIndexesProvider>(sp => sp.GetRequiredService<DosieroIndexFilesWatcher>());
builder.Services.AddSingleton<IFilePricer, DosieroIndexesBasedFilePricer>();
builder.Services.AddSingleton<IPaymentIntegration, MoneroPaymentIntegration>();
builder.Services
    .AddHttpClient<WalletRpcClient>()
    .ConfigureHttpClient((services, client) =>
    {
        var options = services.GetRequiredService<IOptions<DosieroOptions>>();
        client.BaseAddress = options.Value.WalletRpcUri;

    })
    .ConfigurePrimaryHttpMessageHandler(services =>
    {
        var options = services.GetRequiredService<IOptions<DosieroOptions>>();
        var handler = new HttpClientHandler();

        if (options.Value.WalletRpcAcceptSelfSignedCerts is true)
        {
            handler.ServerCertificateCustomValidationCallback += (_, _, _, _) => true;
        }

        if (options.Value is { WalletRpcUserName: { } username, WalletRpcUri: var baseUri, WalletRpcPassword: var password })
        {
            var credentials = new CredentialCache
            {
                { baseUri, "Digest", new NetworkCredential(username, password) },
            };
            handler.PreAuthenticate = false;
            handler.Credentials = credentials;
        }

        return handler;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets().ShortCircuit();

app.UseDirectoryBrowser();

app.MapGet("/", async context =>
{
    context.Response.Redirect("/files");
    await Task.CompletedTask;
});

app.MapGet("/buy", async (HttpContext context, [AsParameters] BuyParameters request, CancellationToken token = default) =>
{
    var file = request.FileProvider.GetFileInfo(request.FileName);

    if (!file.Exists || file.PhysicalPath is null)
    {
        return Results.NotFound();
    }

    var pricing = request.FilePricer.GetFilePrice(file);
    if (pricing is not FilePrice.Paid price)
    {
        return Results.File(file.PhysicalPath, fileDownloadName: file.Name, enableRangeProcessing: true);
    }

    /* TODO: represent files as uris i.e fs:foo.png */
    var payment = await request.PaymentIntegration.CreatePaymentAsync(new CreatePaymentParameters("test", request.FileName, price), token);
    var paymentUri = await request.PaymentIntegration.ToUriAsync(payment);
    var paymentUriBase64 = Encoding.UTF8.GetString(Base64Url.EncodeToUtf8(Encoding.UTF8.GetBytes(paymentUri.ToString())));

    return Results.Redirect($"/pay?payment={paymentUriBase64}");
});

app.MapGet("/pay", async (HttpContext context, [AsParameters] PayParameters request, CancellationToken token = default) =>
{
    /* TODO: Create model binder for this, payment integration based off scheme */
    var paymentUriText = Encoding.UTF8.GetString(Base64Url.DecodeFromUtf8(Encoding.UTF8.GetBytes(request.PaymentUriBase64)));
    var paymentUri = new Uri(paymentUriText);
    var payment = await request.PaymentIntegration.FromUriAsync(paymentUri, token);
    return TypedResults.Blazor<App>(NavigationInit.ForHttpContext(context), [RenderParameter.For("Payment", payment)]);
});

app.MapGet("/download", async (HttpContext context, [AsParameters] DownloadParameters request, CancellationToken token = default) =>
{
    /* TODO: Create model binder for this, payment integration based off scheme */
    var paymentUriText = Encoding.UTF8.GetString(Base64Url.DecodeFromUtf8(Encoding.UTF8.GetBytes(request.PaymentUriBase64)));
    var paymentUri = new Uri(paymentUriText);
    var payment = await request.PaymentIntegration.FromUriAsync(paymentUri, token);

    var file = request.FileProvider.GetFileInfo(payment.FileName);

    if (!file.Exists || file.PhysicalPath is null)
    {
        return Results.NotFound();
    }

    return Results.File(file.PhysicalPath, fileDownloadName: file.Name, enableRangeProcessing: true);
});

var watcher = app.Services.GetRequiredService<IDosieroIndexesWatcher>();

await Task.WhenAny([
    watcher.WatchAsync(app.Lifetime.ApplicationStopping),
    app.RunAsync(),
]);

file sealed class BuyParameters
{
    [FromServices]
    public required IFileProvider FileProvider { get; set; }

    [FromServices]
    public required IPaymentIntegration PaymentIntegration { get; set; }

    [FromServices]
    public required IFilePricer FilePricer { get; set; }

    [FromQuery(Name = "file")]
    public required string FileName { get; set; }
}

file sealed class PayParameters
{
    [FromServices]
    public required IFileProvider FileProvider { get; set; }

    [FromServices]
    public required IPaymentIntegration PaymentIntegration { get; set; }

    [FromServices]
    public required IFilePricer FilePricer { get; set; }

    [FromQuery(Name = "payment")]
    public required string PaymentUriBase64 { get; set; }
}

file sealed class DownloadParameters
{
    [FromServices]
    public required IFileProvider FileProvider { get; set; }

    [FromServices]
    public required IPaymentIntegration PaymentIntegration { get; set; }

    [FromQuery(Name = "payment")]
    public required string PaymentUriBase64 { get; set; }
}
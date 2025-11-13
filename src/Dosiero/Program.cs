using Dosiero.Abstractions.Payments;
using Dosiero.FileProviders.FileSystem;
using Dosiero.Integrations.Monero;
using Dosiero;
using Dosiero.Endpoints;

using Microsoft.Extensions.Options;

var builder = WebApplication.CreateSlimBuilder(args);

var configuration = builder.Configuration;

builder.Services
    .AddSingleton<IValidateOptions<DosieroOptions>, DosieroOptionsValidator>()
    .AddOptionsWithValidateOnStart<DosieroOptions>()
    .BindConfiguration(DosieroOptions.Dosiero);
builder.Services.AddSingleton<DosieroFilesWatcher>();
builder.Services.AddSingleton<IDosieroFilesWatcher, DosieroFilesWatcher>(sp => sp.GetRequiredService<DosieroFilesWatcher>());
builder.Services.AddSingleton<IDosieroFilesProvider, DosieroFilesWatcher>(sp => sp.GetRequiredService<DosieroFilesWatcher>());
builder.Services.AddSingleton<IFilePricer, DosieroFileBasedPricer>();
builder.Services.AddSingleton<IFileReadMeProvider, DosieroFileBasedReadmeProvider>();

builder.Services.AddFsFileProvider();
builder.Services.AddMoneroPayments();
builder.Services.AddScoped(sp => sp.GetRequiredKeyedService<IPaymentIntegration>("monero"));

var app = builder.Build();

app.UseStaticFiles();
app.MapBrowseEndpoints();
app.MapPaymentEndpoints();
app.MapDownloadEndpoints();

var watcher = app.Services.GetRequiredService<IDosieroFilesWatcher>();

await Task.WhenAny([
    watcher.WatchAsync(app.Lifetime.ApplicationStopping),
    app.RunAsync(),
]);



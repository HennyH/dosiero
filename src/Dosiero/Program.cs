using Dosiero;
using Dosiero.Abstractions.Payments;
using Dosiero.Endpoints;
using Dosiero.FileProviders.FileSystem;
using Dosiero.Integrations.Monero;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddSingleton<IFilePricer, FilePricer>();
builder.Services.AddSingleton<IFileReadmeProvider, FileReadmeProvider>();
builder.Services.AddScoped(sp => sp.GetRequiredKeyedService<IPaymentIntegration>("monero"));

builder.Services
    .AddDoserioCommand()
    .WithFileProvider<FsFileProviderCommand, FsFileProviderOptions>()
    .WithPaymentMethod<MoneroPaymentMethodCommand, MoneroPaymentOptions>();

var app = builder.Build();

var command = app.Services.GetRequiredService<IDosieroCommand>();
var result = await command.InvokeAsync(args, app.Lifetime.ApplicationStopping);

if (result is DosieroCommandResult.Halt)
{
    return;
}

app.UseStaticFiles();
app.MapBrowseEndpoints();
app.MapPaymentEndpoints();
app.MapDownloadEndpoints();

await app.RunAsync();


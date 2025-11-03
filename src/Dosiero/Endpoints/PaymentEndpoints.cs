using Dosiero.Abstractions.FileProviders;
using Dosiero.Abstractions.Payments;
using Dosiero.Slices.Pages;
using Dosiero.UriUtility;

using Microsoft.AspNetCore.Mvc;

namespace Dosiero.Endpoints;

public static class PaymentEndpoints
{
    private static async Task<IResult> GetBuyAsync([AsParameters] BuyParameters request, CancellationToken token)
    {
        var info = await request.FileProvider.GetFileInfoAsync(request.File, token);

        if (!info.Exists)
        {
            return TypedResults.NotFound();
        }

        var pricing = request.Pricer.GetPrice(info);
        if (pricing is not FilePrice.Paid price)
        {
            var stream = await request.FileProvider.OpenReadStreamAsync(request.File, token);
            return TypedResults.File(stream, fileDownloadName: info.Name, enableRangeProcessing: true);
        }

        var payment = await request.PaymentIntegration.CreatePaymentAsync(new CreatePaymentParameters(request.File, price), token);
        var paymentUri = await request.PaymentIntegration.ToUriAsync(payment, token);
        return Redirects.ToPaymentPage(paymentUri);
    }

    private static async Task<IResult> GetPaymentPageAsync([AsParameters] PayParameters request, CancellationToken token)
    {
        var paymentUri = request.Base64PaymentUri.FromBase64Uri(); 
        var payment = await request.PaymentIntegration.FromUriAsync(paymentUri, token);
        return TypedResults.Extensions.RazorSlice<Pay, IPayment>(payment);
    }

    public static void MapPaymentEndpoints(this WebApplication app)
    {
        app.MapGet("/buy", GetBuyAsync);
        app.MapGet("/pay", GetPaymentPageAsync);
    }

    public static class Redirects
    {
        public static IResult ToPaymentPage(Uri paymentUri)
        {
            return Results.Redirect($"/pay?payment={paymentUri.ToBase64Uri()}");
        }
    }
}

internal sealed class BuyParameters
{
    [FromServices]
    public required IFileProvider FileProvider { get; set; }

    [FromServices]
    public required IFilePricer Pricer { get; set; }

    [FromServices]
    public required IPaymentIntegration PaymentIntegration { get; set; }

    [FromQuery(Name = "file")]
    public required Uri File { get; set; }
}


internal sealed class PayParameters
{
    [FromServices]
    public required IPaymentIntegration PaymentIntegration { get; set; }

    [FromQuery(Name = "payment")]
    public required string Base64PaymentUri { get; set; }
}
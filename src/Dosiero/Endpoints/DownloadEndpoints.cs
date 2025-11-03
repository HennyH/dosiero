using Dosiero.Abstractions.FileProviders;
using Dosiero.Abstractions.Payments;
using Dosiero.UriUtility;

using Microsoft.AspNetCore.Mvc;

namespace Dosiero.Endpoints;

public static class DownloadEndpoints
{
    private static async Task<IResult> GetDownloadFileAsync([AsParameters] DownloadParameters request, CancellationToken token)
    {
        var paymentUri = request.Base64PaymentUri.FromBase64Uri();
        var payment = await request.PaymentIntegration.FromUriAsync(paymentUri, token);

        var hasPaid = await request.PaymentIntegration.HasPaymentBeenMadeAsync(payment, token);
        if (hasPaid is false)
        {
            return PaymentEndpoints.Redirects.ToPaymentPage(paymentUri);
        }

        var info = await request.FileProvider.GetFileInfoAsync(payment.FileUri, token);
        if (!info.Exists)
        {
            return TypedResults.NotFound();
        }

        var stream = await request.FileProvider.OpenReadStreamAsync(payment.FileUri, token);
        return TypedResults.File(stream, fileDownloadName: info.Name, enableRangeProcessing: true);
    }

    public static void MapDownloadEndpoints(this WebApplication app)
    {
        app.MapGet("/download", GetDownloadFileAsync);
    }
}

internal sealed class DownloadParameters
{
    [FromServices]
    public required IFileProvider FileProvider { get; set; }

    [FromServices]
    public required IPaymentIntegration PaymentIntegration { get; set; }

    [FromQuery(Name = "payment")]
    public required string Base64PaymentUri { get; set; }
}
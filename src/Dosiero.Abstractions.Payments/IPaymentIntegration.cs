using Microsoft.AspNetCore.Html;

namespace Dosiero.Abstractions.Payments;

public interface IPaymentIntegration
{
    public string Scheme { get; }

    public Task<IPayment> CreatePaymentAsync(CreatePaymentParameters parameters, CancellationToken token = default);

    public Task<bool> HasPaymentBeenMadeAsync(IPayment payment, CancellationToken token = default);

    public Task<IHtmlContent> RenderPaymentUi(IPayment payment, CancellationToken token = default);

    public Task<Uri> ToUriAsync(IPayment payment, CancellationToken token = default);

    public Task<IPayment> FromUriAsync(Uri uri, CancellationToken token = default);
}

public interface IPaymentIntegration<TIntegration, TPayment> : IPaymentIntegration
    where TIntegration : IPaymentIntegration<TIntegration, TPayment>
    where TPayment : IPayment
{
    new public static abstract string Scheme { get; }

    new public Task<TPayment> CreatePaymentAsync(CreatePaymentParameters parameters, CancellationToken token = default);

    public Task<bool> HasPaymentBeenMadeAsync(TPayment payment, CancellationToken token = default);

    public Task<IHtmlContent> RenderPaymentUi(TPayment payment, CancellationToken token = default);

    public Task<Uri> ToUriAsync(TPayment payment, CancellationToken token = default);

    new public Task<TPayment> FromUriAsync(Uri uri, CancellationToken token = default);

    string IPaymentIntegration.Scheme => TIntegration.Scheme;

    async Task<IPayment> IPaymentIntegration.CreatePaymentAsync(CreatePaymentParameters parameters, CancellationToken token)
        =>  await CreatePaymentAsync(parameters, token);

    Task<bool> IPaymentIntegration.HasPaymentBeenMadeAsync(IPayment payment, CancellationToken token)
        => HasPaymentBeenMadeAsync((TPayment)payment, token);

    Task<IHtmlContent> IPaymentIntegration.RenderPaymentUi(IPayment payment, CancellationToken token)
        => RenderPaymentUi((TPayment)payment, token);

    Task<Uri> IPaymentIntegration.ToUriAsync(IPayment payment, CancellationToken token)
        => ToUriAsync((TPayment)payment, token);

    async Task<IPayment> IPaymentIntegration.FromUriAsync(Uri uri, CancellationToken token)
        => await FromUriAsync(uri, token);
}
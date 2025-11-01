using Microsoft.AspNetCore.Components;

namespace Dosiero;

public interface IPaymentIntegration
{
    public string Scheme { get; }

    public Task<IPayment> CreatePaymentAsync(CreatePaymentParameters parameters, CancellationToken token = default);

    public Task<bool> HasPaymentBeenMadeAsync(IPayment payment, CancellationToken token = default);

    public Task<MarkupString> RenderPaymentUi(IPayment payment, CancellationToken token = default);

    public Task<Uri> ToUriAsync(IPayment payment, CancellationToken token = default);

    public Task<IPayment> FromUriAsync(Uri uri, CancellationToken token = default);
}

public interface IPaymentIntegration<TPayment> : IPaymentIntegration
    where TPayment : IPayment
{
    new public Task<TPayment> CreatePaymentAsync(CreatePaymentParameters parameters, CancellationToken token = default);

    public Task<bool> HasPaymentBeenMadeAsync(TPayment payment, CancellationToken token = default);

    public Task<MarkupString> RenderPaymentUi(TPayment payment, CancellationToken token = default);

    public Task<Uri> ToUriAsync(TPayment uri, CancellationToken token = default);

    new public Task<TPayment> FromUriAsync(Uri uri, CancellationToken token = default);

    async Task<Uri> IPaymentIntegration.ToUriAsync(IPayment payment, CancellationToken token)
        => await ToUriAsync((TPayment)payment, token);

    async Task<IPayment> IPaymentIntegration.FromUriAsync(Uri uri, CancellationToken token)
        => await FromUriAsync(uri, token);

    async Task<IPayment> IPaymentIntegration.CreatePaymentAsync(CreatePaymentParameters parameters, CancellationToken token)
        => await CreatePaymentAsync(parameters, token);

    Task<bool> IPaymentIntegration.HasPaymentBeenMadeAsync(IPayment payment, CancellationToken token)
        => HasPaymentBeenMadeAsync((TPayment)payment, token);

    Task<MarkupString> IPaymentIntegration.RenderPaymentUi(IPayment payment, CancellationToken token)
        => RenderPaymentUi((TPayment)payment, token);
}

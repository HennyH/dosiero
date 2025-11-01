using static Dosiero.FilePrice;

namespace Dosiero;

public sealed record CreatePaymentParameters(string PaymentId, string FileName, Paid FilePrice);
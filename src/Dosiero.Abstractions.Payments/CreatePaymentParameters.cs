using static Dosiero.Abstractions.Payments.FilePrice;

namespace Dosiero.Abstractions.Payments;

public sealed record CreatePaymentParameters(Uri FileUri, Paid FilePrice);
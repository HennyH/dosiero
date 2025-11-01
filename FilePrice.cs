namespace Dosiero;

public abstract record FilePrice
{
    public sealed record Free : FilePrice;

    public sealed record Paid(decimal Price) : FilePrice;
}
using Dosiero.Abstractions.FileProviders;
using Dosiero.Abstractions.Payments;

namespace Dosiero;

internal sealed class FilePricer : IFilePricer
{
    private readonly List<FilePricing> _pricings = [];
    private readonly Dictionary<Uri, FilePrice> _cache = [];

    public FilePrice GetPrice(IFileInfo file)
    {
        if (_cache.TryGetValue(file.Uri, out var cached))
        {
            return cached;
        }

        foreach (var (pattern, price) in _pricings)
        {
            if (pattern.IsMatch(file.Uri.AbsolutePath))
            {
                return _cache[file.Uri] = price;
            }
        }

        return new FilePrice.Free();
    }

    public void SetPrice(LikeString pattern, FilePrice price)
    {
        _pricings.Add(new FilePricing(pattern, price));
        _cache.Clear();
    }
    
    private sealed record FilePricing(LikeString Pattern, FilePrice Price);
}

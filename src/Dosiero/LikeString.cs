using System.Text.RegularExpressions;

namespace Dosiero;

internal sealed record LikeString(string Pattern)
{
    public LikeMatchResult Match(string text)
    {
        if (!Pattern.Contains('%'))
        {
            return string.Equals(text, Pattern, StringComparison.OrdinalIgnoreCase)
                ? LikeMatchResult.Match([])
                : LikeMatchResult.None;
        }

        var regex = new Regex(Regex.Escape(Pattern).Replace("%", "(.*)"));
        var match = regex.Match(text);
        return new LikeMatchResult(match.Success, [..match.Groups.Values.Select(g => g.Value)]);
    }

    public bool IsMatch(string text) => Match(text).IsMatch;
}

internal sealed record LikeMatchResult(bool IsMatch, IReadOnlyList<string> Captures)
{
    public static LikeMatchResult Match(IReadOnlyList<string> captures)
        => new(IsMatch: true, Captures: captures);

    public static readonly LikeMatchResult None = new(IsMatch: false, Captures: []);
}

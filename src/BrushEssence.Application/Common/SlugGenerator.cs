using System.Text.RegularExpressions;

namespace BrushEssence.Application.Common;

/// <summary>Turns a display name into a URL-friendly slug.</summary>
public static partial class SlugGenerator
{
    public static string Generate(string input)
    {
        var slug = input.Trim().ToLowerInvariant();
        slug = NonSlugChars().Replace(slug, "");
        slug = Separators().Replace(slug, "-").Trim('-');
        return slug;
    }

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex NonSlugChars();

    [GeneratedRegex(@"[\s-]+")]
    private static partial Regex Separators();
}

namespace BrushEssence.Application.Orders;

/// <summary>
/// Produces human-friendly, hard-to-guess order references like
/// <c>BE-20260626-3F7A1C</c>: a date for at-a-glance recency plus six random hex
/// characters for uniqueness. Uniqueness is additionally guaranteed by a unique
/// index; the service retries on the astronomically rare collision.
/// </summary>
public static class OrderNumberGenerator
{
    public static string Generate(DateTimeOffset now)
    {
        var datePart = now.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"BE-{datePart}-{randomPart}";
    }
}

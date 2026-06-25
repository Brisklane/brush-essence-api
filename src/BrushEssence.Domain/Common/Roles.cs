namespace BrushEssence.Domain.Common;

/// <summary>
/// Canonical role names. Used for seeding, claims, and authorization policies so
/// the strings are defined in exactly one place.
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";

    public static readonly IReadOnlyList<string> All = [Admin, Customer];
}

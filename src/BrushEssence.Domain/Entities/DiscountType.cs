namespace BrushEssence.Domain.Entities;

/// <summary>How a promotion's discount is calculated. Stored as a string.</summary>
public enum DiscountType
{
    /// <summary>A percentage off the price (Value is 0–100).</summary>
    Percentage = 0,

    /// <summary>A fixed amount off the price, in the store currency.</summary>
    FixedAmount = 1,
}

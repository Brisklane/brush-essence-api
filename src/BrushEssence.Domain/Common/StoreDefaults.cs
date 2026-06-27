namespace BrushEssence.Domain.Common;

/// <summary>
/// Store-wide fixed defaults. The shop operates in a single currency (PKR);
/// it is set server-side and is never taken from client input, so prices,
/// carts, orders and commissions are always expressed in the same currency.
/// </summary>
public static class StoreDefaults
{
    /// <summary>ISO 4217 code of the only currency the store transacts in.</summary>
    public const string Currency = "PKR";
}

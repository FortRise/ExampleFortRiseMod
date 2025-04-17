namespace Teuria.AdditionalVariants;

/// <summary>
/// API for most variants in AdditionalVariants
/// </summary>
public partial interface IAdditionalVariantsAPI 
{
    /// <summary>
    /// Contains API for DashStamina variants.
    /// </summary>
    public IDashStaminaAPI DashStamina { get; }
    /// <summary>
    /// Contains API for Jester's Hat Variant
    /// </summary>
    public IJesterHatAPI JesterHat { get; }
}

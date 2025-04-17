namespace Teuria.AdditionalVariants;

internal partial class ApiImplementation : IAdditionalVariantsAPI
{
    public ApiImplementation()
    {
        DashStamina = new DashStaminaImplementation();
        JesterHat = new JesterHatImplementation();
    }

    public IAdditionalVariantsAPI.IDashStaminaAPI DashStamina { get; set; }
    public IAdditionalVariantsAPI.IJesterHatAPI JesterHat { get; set; }
}

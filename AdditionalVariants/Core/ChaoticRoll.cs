namespace AdditionalVariants;

public static class ChaoticRoll 
{
    public static void Load() 
    {
        On.TowerFall.RoundLogic.ctor += ctor_patch;
    }

    public static void Unload() 
    {
        On.TowerFall.RoundLogic.ctor -= ctor_patch;
    }

    private static void ctor_patch(On.TowerFall.RoundLogic.orig_ctor orig, TowerFall.RoundLogic self, TowerFall.Session session, bool canHaveMiasma)
    {
        orig(self, session, canHaveMiasma);
        if (session.MatchSettings.Variants.GetCustomVariant("AdditionalVariants/ChaoticRoll")) 
            session.MatchSettings.Variants.Randomize();
    }
}
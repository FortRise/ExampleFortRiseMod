using FortRise;
using HarmonyLib;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class ChaoticRoll : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(
                typeof(RoundLogic),
                [typeof(Session), typeof(bool)]
            ),
            postfix: new HarmonyMethod(RoundLogic_ctor_Postfix)
        );
    }

    private static void RoundLogic_ctor_Postfix(Session session)
    {
        if (Variants.ChaoticRoll.IsActive()) 
        {
            session.MatchSettings.Variants!.Randomize();
        }
    }
}
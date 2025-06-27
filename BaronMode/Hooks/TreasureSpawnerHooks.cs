using FortRise;
using HarmonyLib;
using Teuria.BaronMode.Pickups;
using TowerFall;

namespace Teuria.BaronMode.Hooks;

public class TreasureSpawnerHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(TreasureSpawner), [typeof(Session), typeof(VersusTowerData)]),
            postfix: new HarmonyMethod(TreasureSpawner_ctor_Postfix)
        );
    }

    private static void TreasureSpawner_ctor_Postfix(TowerFall.TreasureSpawner __instance)
    {
        var gemLivesPickup = GemLives.GemLivesMeta.Pickups;

        if (BaronMatchVariants.TreasureLivesInfo.IsActive(__instance.Session.MatchSettings.Variants))
        {
            __instance.TreasureRates[(int)gemLivesPickup] = 1f;
        }
        else 
        {
            // Let's make sure it won't spawn at all
            __instance.TreasureRates[(int)gemLivesPickup] = 0f;
        }
    }
}
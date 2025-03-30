using BaronMode.Pickups;
using FortRise;

namespace BaronMode.Hooks;

public static class TreasureSpawnerHooks 
{
    public static void Load()
    {
        On.TowerFall.TreasureSpawner.ctor_Session_VersusTowerData += PutLivesTreasure;
    }

    public static void Unload()
    {
        On.TowerFall.TreasureSpawner.ctor_Session_VersusTowerData -= PutLivesTreasure;
    }

    private static void PutLivesTreasure(On.TowerFall.TreasureSpawner.orig_ctor_Session_VersusTowerData orig, TowerFall.TreasureSpawner self, TowerFall.Session session, TowerFall.VersusTowerData versusTowerData)
    {
        orig(self, session, versusTowerData);
        if (session.MatchSettings.Variants.GetCustomVariant("BaronMode/TreasureLives"))
        {
            self.TreasureRates[(int)ModRegisters.PickupType<GemLives>()] = 1f;
        }
        else 
        {
            // Let's make sure it won't spawn at all
            self.TreasureRates[(int)ModRegisters.PickupType<GemLives>()] = 0f;
        }
    }
}
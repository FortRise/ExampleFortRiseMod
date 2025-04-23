using Teuria.BaronMode.Pickups;

namespace Teuria.BaronMode.Hooks;

public class TreasureSpawnerHooks : IHookable
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
        var gemLivesPickup = GemLives.GemLivesMeta.Pickups;

        if (BaronMatchVariants.TreasureLivesInfo.IsActive(self.Session.MatchSettings.Variants))
        {
            self.TreasureRates[(int)gemLivesPickup] = 1f;
        }
        else 
        {
            // Let's make sure it won't spawn at all
            self.TreasureRates[(int)gemLivesPickup] = 0f;
        }
    }
}
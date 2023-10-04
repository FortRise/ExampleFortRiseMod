using FortRise;
using Monocle;
using TowerFall;

namespace AdditionalVariants;

public static class PlayerDeathVariants 
{
    public static void Load() 
    {
        On.TowerFall.Player.Die_DeathCause_int_bool_bool += DeathVariants;
    }

    public static void Unload() 
    {
        On.TowerFall.Player.Die_DeathCause_int_bool_bool -= DeathVariants;
    }

    private static TowerFall.PlayerCorpse DeathVariants(On.TowerFall.Player.orig_Die_DeathCause_int_bool_bool orig, TowerFall.Player self, DeathCause deathCause, int killerIndex, bool brambled, bool laser)
    {
        if (VariantManager.GetCustomVariant("ShockDeath")[self.PlayerIndex]) 
        {
            ShockCircle shockCircle = Cache.Create<ShockCircle>();
            shockCircle.Init(self.Position, self.PlayerIndex, self, ShockCircle.ShockTypes.BoltCatch);
            self.Level.Add(shockCircle);
            Sounds.sfx_reviveRedteamFinish.Play(self.X, 1f);
        }
        if (VariantManager.GetCustomVariant("ChestDeath")[self.PlayerIndex]) 
        {
            var posY = self.Position with { Y = self.Position.Y - 5 };
            TreasureChest chest;
            if (self.Level.Session.MatchSettings.Variants.BombChests) 
            {
                chest = new TreasureChest(posY, TreasureChest.Types.Normal, TreasureChest.AppearModes.Normal, Pickups.Bomb, 0);
            }
            else 
            {
                var treasureSpawns = self.Level.Session.TreasureSpawner.GetTreasureSpawn();
                chest = new TreasureChest(posY, TreasureChest.Types.AutoOpen, TreasureChest.AppearModes.Time, treasureSpawns, 30);
            }

            self.Level.Add(chest);
        }

        return orig(self, deathCause, killerIndex, brambled, laser);
    }
}
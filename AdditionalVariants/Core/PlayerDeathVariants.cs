using System;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
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
        if (VariantManager.GetCustomVariant("AdditionalVariants/ShockDeath")[self.PlayerIndex]) 
        {
            ShockCircle shockCircle = Cache.Create<ShockCircle>();
            shockCircle.Init(self.Position, self.PlayerIndex, self, ShockCircle.ShockTypes.BoltCatch);
            self.Level.Add(shockCircle);
            Sounds.sfx_reviveRedteamFinish.Play(self.X, 1f);
        }
        if (VariantManager.GetCustomVariant("AdditionalVariants/ChestDeath")[self.PlayerIndex]) 
        {
            var x = (float)(Math.Floor(self.Position.X / 10.0f) * 10.0f);
            var y = (float)(Math.Floor((self.Position.Y - 5) / 10.0f) * 10.0f);
            var position = new Vector2(x + 5, y);
            TreasureChest chest;
            if (self.Level.Session.MatchSettings.Variants.BombChests) 
            {
                chest = new TreasureChest(position, TreasureChest.Types.Normal, TreasureChest.AppearModes.Normal, Pickups.Bomb, 0);
            }
            else 
            {
                var treasureSpawns = self.Level.Session.TreasureSpawner.GetTreasureSpawn();
                chest = new TreasureChest(position, TreasureChest.Types.AutoOpen, TreasureChest.AppearModes.Time, treasureSpawns, 30);
            }
            var texture = self.TeamColor switch
            {
                Allegiance.Neutral => TextureRegistry.GrayChest,
                Allegiance.Blue => TextureRegistry.BlueChest,
                Allegiance.Red => TextureRegistry.RedChest,
                _ => TFGame.Atlas["treasureChestSpecial"],
            };
            DynamicData.For(chest).Get<Sprite<int>>("sprite")!.SwapSubtexture(texture);

            self.Level.Add(chest);
        }

        return orig(self, deathCause, killerIndex, brambled, laser);
    }
}
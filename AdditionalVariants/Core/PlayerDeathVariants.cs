using System;
using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.AdditionalVariants;


public class PlayerDeathVariants : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(
                typeof(Player),
                nameof(Player.Die),
                [typeof(DeathCause), typeof(int), typeof(bool), typeof(bool)]
            ),
            new HarmonyMethod(Player_Die_Prefix)
        );
    }

    private static void Player_Die_Prefix(Player __instance)
    {
        if (Variants.ShockDeath.IsActive(__instance.PlayerIndex)) 
        {
            ShockCircle shockCircle = Cache.Create<ShockCircle>();
            shockCircle.Init(__instance.Position, __instance.PlayerIndex, __instance, ShockCircle.ShockTypes.BoltCatch);
            __instance.Level.Add(shockCircle);
            Sounds.sfx_reviveRedteamFinish.Play(__instance.X, 1f);
        }
        if (Variants.ChestDeath.IsActive(__instance.PlayerIndex)) 
        {
            var x = (float)(Math.Floor(__instance.Position.X / 10.0f) * 10.0f);
            var y = (float)(Math.Floor((__instance.Position.Y - 5) / 10.0f) * 10.0f);
            var position = new Vector2(x + 5, y);
            TreasureChest chest;
            if (__instance.Level.Session.MatchSettings.Variants.BombChests) 
            {
                chest = new TreasureChest(position, TreasureChest.Types.Normal, TreasureChest.AppearModes.Normal, Pickups.Bomb, 0);
            }
            else 
            {
                var treasureSpawns = __instance.Level.Session.TreasureSpawner.GetTreasureSpawn();
                chest = new TreasureChest(position, TreasureChest.Types.AutoOpen, TreasureChest.AppearModes.Time, treasureSpawns, 30);
            }
            var texture = __instance.TeamColor switch
            {
                Allegiance.Neutral => TextureRegistry.GrayChest.Subtexture,
                Allegiance.Blue => TextureRegistry.BlueChest.Subtexture,
                Allegiance.Red => TextureRegistry.RedChest.Subtexture,
                _ => TFGame.Atlas["treasureChestSpecial"],
            };
            DynamicData.For(chest).Get<Sprite<int>>("sprite")!.SwapSubtexture(texture);

            __instance.Level.Add(chest);
        }
    }
}
using System.Collections.Generic;
using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class JesterHat : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), nameof(Player.Added)),
            postfix: new HarmonyMethod(Player_Added_Postfix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), "LeaveDodge"),
            postfix: new HarmonyMethod(Player_LeaveDodge_Prefix)
        );
    }

    private static void Player_Added_Postfix(Player __instance)
    {
        if (Variants.JestersHat.IsActive(__instance.PlayerIndex))
        {
            var warpPoints = new List<Vector2>();
            var playerSpawn = __instance.Level.GetXMLPositions("PlayerSpawn");
            var teamSpawn = __instance.Level.GetXMLPositions("TeamSpawn");
            var teamSpawnA = __instance.Level.GetXMLPositions("TeamSpawnA");
            var teamSpawnB = __instance.Level.GetXMLPositions("TeamSpawnB");
            var spawner = __instance.Level.GetXMLPositions("Spawner");
            var treasureChest = __instance.Level.GetXMLPositions("TreasureChest");
            warpPoints.AddRange(playerSpawn);
            warpPoints.AddRange(teamSpawn);
            warpPoints.AddRange(teamSpawnA);
            warpPoints.AddRange(teamSpawnB);
            warpPoints.AddRange(spawner);
            warpPoints.AddRange(treasureChest);
            foreach (var hook in JesterHatManager.Instance.Hooks)
            {
                hook.ModifyWarpPoints(new ApiImplementation.JesterHatImplementation.ModifyWarpPointsArgs(__instance, warpPoints));
            }
            DynamicData.For(__instance).Set("warpPoints", warpPoints);
            DynamicData.For(__instance).Set("lastWarpPoint", new Vector2(0, 0));
        }
    }

    private static void Player_LeaveDodge_Prefix(Player __instance)
    {
        var dynSelf = DynamicData.For(__instance);
        if (dynSelf.TryGet<List<Vector2>>("warpPoints", out var warpPoints)) 
        {
            DoExplodeEffect(__instance);
            Sounds.sfx_cyanWarp.Play(__instance.X, 1f);
            var lightFade = Cache.Create<LightFade>();
            lightFade.Init(__instance, null);
            __instance.Level.Add(lightFade);
            warpPoints.Sort((x, y) => WarpSorter(__instance, x, y));
            var warp = warpPoints[1];
            var lastWarpPoint = dynSelf.Get<Vector2>("lastWarpPoint");
            if (warp == lastWarpPoint) 
            {
                warp = warpPoints[0];
            }
            __instance.Position = warp;
            __instance.Level.Particles.Emit(Particles.PlayerDust[__instance.CharacterIndex], 12, __instance.Position, new Vector2(5f, 8f));

            foreach (var hook in JesterHatManager.Instance.Hooks)
            {
                hook.AfterTeleport(new ApiImplementation.JesterHatImplementation.AfterTeleportArgs(__instance, warp));
            }

            dynSelf.Set("lastWarpPoint", warp);
        }
    }

    private static int WarpSorter(Player player, Vector2 a, Vector2 b) 
    {
        if (Vector2.DistanceSquared(a, player.Position) <= 400f)
        {
            return 1;
        }
        if (Vector2.DistanceSquared(b, player.Position) <= 400f)
        {
            return -1;
        }

        return (int)(WrapMath.WrapDistanceSquared(a, player.Position) - WrapMath.WrapDistanceSquared(b, player.Position));
    }

    private static void DoExplodeEffect(Player player)
    {
        var entity = new Entity(0)
        {
            Position = player.Position,
            Depth = player.Depth - 1
        };
        Sprite<int> sprite = TFGame.SpriteData.GetSpriteInt("ShadowExplosion");
        sprite.Play(0, false);
        sprite.OnAnimationComplete = _ =>
        {
            entity.RemoveSelf();
        };
        entity.Add(sprite);
        player.Level.Add<Entity>(entity);
    }
}
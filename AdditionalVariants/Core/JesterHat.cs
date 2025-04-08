using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class JesterHat : IHookable
{
    public static void Load() 
    {
        On.TowerFall.Player.LeaveDodge += JesterHatVariant;
        On.TowerFall.Player.Added += JesterHatAdded;
    }

    public static void Unload() 
    {
        On.TowerFall.Player.LeaveDodge -= JesterHatVariant;
        On.TowerFall.Player.Added -= JesterHatAdded;
    }

    private static void JesterHatAdded(On.TowerFall.Player.orig_Added orig, Player self)
    {
        orig(self);
        if (Variants.JestersHat.IsActive(self.PlayerIndex))
        {
            var warpPoints = new List<Vector2>();
            var playerSpawn = self.Level.GetXMLPositions("PlayerSpawn");
            var teamSpawn = self.Level.GetXMLPositions("TeamSpawn");
            var teamSpawnA = self.Level.GetXMLPositions("TeamSpawnA");
            var teamSpawnB = self.Level.GetXMLPositions("TeamSpawnB");
            var spawner = self.Level.GetXMLPositions("Spawner");
            var treasureChest = self.Level.GetXMLPositions("TreasureChest");
            warpPoints.AddRange(playerSpawn);
            warpPoints.AddRange(teamSpawn);
            warpPoints.AddRange(teamSpawnA);
            warpPoints.AddRange(teamSpawnB);
            warpPoints.AddRange(spawner);
            warpPoints.AddRange(treasureChest);
            DynamicData.For(self).Set("warpPoints", warpPoints);
            DynamicData.For(self).Set("lastWarpPoint", new Vector2(0, 0));
        }
    }

    private static void JesterHatVariant(On.TowerFall.Player.orig_LeaveDodge orig, Player self)
    {
        var dynSelf = DynamicData.For(self);
        if (dynSelf.TryGet<List<Vector2>>("warpPoints", out var warpPoints)) 
        {
            DoExplodeEffect(self);
            Sounds.sfx_cyanWarp.Play(self.X, 1f);
            var lightFade = Cache.Create<LightFade>();
            lightFade.Init(self, null);
            self.Level.Add(lightFade);
            warpPoints.Sort((x, y) => WarpSorter(self, x, y));
            var warp = warpPoints[1];
            var lastWarpPoint = dynSelf.Get<Vector2>("lastWarpPoint");
            if (warp == lastWarpPoint) 
            {
                warp = warpPoints[0];
            }
            self.Position = warp;
            self.Level.Particles.Emit(Particles.PlayerDust[self.CharacterIndex], 12, self.Position, new Vector2(5f, 8f));
            dynSelf.Set("lastWarpPoint", warp);
        }
        orig(self);
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
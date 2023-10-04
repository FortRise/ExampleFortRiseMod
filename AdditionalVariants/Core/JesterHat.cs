using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace AdditionalVariants;

public static class JesterHat 
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
        if (VariantManager.GetCustomVariant("JestersHat")[self.PlayerIndex]) 
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
            self.Position = warpPoints[1];
            self.Level.Particles.Emit(Particles.PlayerDust[self.CharacterIndex], 12, self.Position, new Vector2(5f, 8f));
        }
        orig(self);
    }

    private static int WarpSorter(Player player, Vector2 a, Vector2 b)
    {
        if (VisionCone(player.Facing, 1.7453293f, a)) 
        {
            return 1;
        }
        if (Vector2.DistanceSquared(a, player.Position) <= 400f)
        {
            return 1;
        }
        if (Vector2.DistanceSquared(b, player.Position) <= 400f)
        {
            return -1;
        }
        return 0;

		bool VisionCone(Facing facing, float range, Vector2 check)
		{
			return Calc.AbsAngleDiff((facing == Facing.Left) ? 3.1415927f : 0f, WrapMath.WrapAngle(player.Position, check)) <= range;
		}
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
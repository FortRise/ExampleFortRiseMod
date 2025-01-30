using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace AdditionalVariants;

public static class PlayerStaminaHooks 
{
    public static void Load() 
    {
        On.TowerFall.Player.Added += AddStamina;
        On.TowerFall.Player.HUDRender += HUDRender;
        On.TowerFall.Player.EnterDodge += DodgeEnter;
    }

    public static void Unload() 
    {
        On.TowerFall.Player.Added -= AddStamina;
        On.TowerFall.Player.HUDRender -= HUDRender;
        On.TowerFall.Player.EnterDodge -= DodgeEnter;
    }

    private static void HUDRender(On.TowerFall.Player.orig_HUDRender orig, Player self, bool wrapped)
    {
        orig(self, wrapped);
        if (self.Dynamic().TryGet<DashStamina>("dashStamina", out var stamina)) 
        {
            stamina.Render();
        }
    }

    private static void DodgeEnter(On.TowerFall.Player.orig_EnterDodge orig, Player self)
    {
        if (self.Dynamic().TryGet<DashStamina>("dashStamina", out var stamina)) 
        {
            if (stamina.UseSmallStamina()) 
            {
                orig(self);
                return;
            }

            self.State = Player.PlayerStates.Normal;
            return;
        }

        orig(self);
    }

    private static void AddStamina(On.TowerFall.Player.orig_Added orig, Player self)
    {
        orig(self);
        if (VariantManager.GetCustomVariant("DashStamina")) 
        {
            var dashStamina = new DashStamina(true, true);
            self.Add(dashStamina);
            self.DynSetData("dashStamina", dashStamina);
        }
    }
}

public class DashStamina : Component
{
    private Subtexture staminaImage;
    private Vector2 staminaPosition;
    private float staminaBar;
    private float alpha;
    public DashStamina(bool active, bool visible) : base(active, visible)
    {
    }

    public bool UseSmallStamina() 
    {
        if (staminaBar >= 0.5f) 
        {
            staminaBar -= 0.5f;
            return true;
        }
        return false;
    }

    public override void Added()
    {
        base.Added();
        staminaImage = TextureRegistry.StaminaBar;
    }

    public override void Update()
    {
        base.Update();
        staminaPosition.X = Entity.X - 8;
        staminaPosition.Y = Entity.Y - 26;
        if (staminaBar >= 1) 
        {
            staminaBar = 1;
            if (alpha > 0) 
            {
                alpha -= Engine.TimeMult * 0.01f;
                return;
            }
            alpha = 0;
            return;
        }
        staminaBar += Engine.TimeMult * 0.005f;
        alpha = 1;
    }

    public override void Render()
    {
        base.Render();
        var color = staminaBar switch {
            < 0.5f => Color.Red,
            < 0.7f => Color.Yellow,
            < 0.9f => Color.YellowGreen,
            _ => Color.Green
        } * alpha;
        Draw.Rect(Entity.X - 8, Entity.Y - 26, (staminaBar * 20), 5, color);
        Draw.Texture(staminaImage, staminaPosition, Color.White * alpha);
    }
}

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
            DynamicData.For(chest).Get<Sprite<int>>("sprite").SwapSubtexture(texture);

            self.Level.Add(chest);
        }

        return orig(self, deathCause, killerIndex, brambled, laser);
    }
}
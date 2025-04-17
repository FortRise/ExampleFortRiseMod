using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class PlayerStamina : IHookable
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
        if (DynamicData.For(self).TryGet<DashStamina>("dashStamina", out var stamina)) 
        {
            stamina.Render();
        }
    }

    private static void DodgeEnter(On.TowerFall.Player.orig_EnterDodge orig, Player self)
    {
        if (DynamicData.For(self).TryGet<DashStamina>("dashStamina", out var stamina)) 
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
        if (Variants.DashStamina.IsActive(self.PlayerIndex))
        {
            var dashStamina = new DashStamina(true, true);
            self.Add(dashStamina);
            DynamicData.For(self).Set("dashStamina", dashStamina);
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
        staminaImage = TextureRegistry.StaminaBar;
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

    public bool CanUseStamina => staminaBar >= 0.5f;

    public override void Update()
    {
        base.Update();
        staminaPosition.X = Entity.X - 8;
        staminaPosition.Y = Entity.Y - 26;
        if (staminaBar >= 1) 
        {
            staminaBar = 1;
            alpha -= Math.Max(0, Engine.TimeMult * 0.01f);
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
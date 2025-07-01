using System;
using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using Teuria.Ascencore;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class PlayerStamina : IHookable, IAscencoreAPI.IPlayerDodgeStateHookApi.IHook
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), nameof(Player.Added)),
            postfix: new HarmonyMethod(Player_Added_Postfix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), nameof(Player.HUDRender)),
            postfix: new HarmonyMethod(Player_HUDRender_Postfix)
        );

        AdditionalVariantsModule.AscencoreAPI.DodgeStateApi.RegisterHook(
            new PlayerStamina(),
            -10
        );
    }

    public Option<bool> IsDodgeEnabled(IAscencoreAPI.IPlayerDodgeStateHookApi.IHook.IsDodgeEnabledEventArgs args)
    {
        if (DynamicData.For(args.Player).TryGet<DashStamina>("dashStamina", out var stamina))
        {
            if (stamina.UseSmallStamina())
            {
                return true;
            }

            return false;
        }

        return Option<bool>.None();
    }

    private static void Player_HUDRender_Postfix(Player __instance)
    {
        if (DynamicData.For(__instance).TryGet<DashStamina>("dashStamina", out var stamina))
        {
            stamina.Render();
        }
    }

    private static void Player_Added_Postfix(Player __instance)
    {
        if (Variants.DashStamina.IsActive(__instance.PlayerIndex))
        {
            var dashStamina = new DashStamina(true, true);
            __instance.Add(dashStamina);
            DynamicData.For(__instance).Set("dashStamina", dashStamina);
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
        staminaImage = TextureRegistry.StaminaBar.Subtexture!;
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
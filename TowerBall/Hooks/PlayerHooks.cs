using System;
using System.Collections.Generic;
using FortRise;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace TowerBall;

[HarmonyPatch(typeof(Player))]
public static class PlayerHooks
{
    public static Dictionary<int, bool> touchedGroundSinceCollect = new Dictionary<int, bool>(16);

    public static Dictionary<int, float> currentHoldFrames = new Dictionary<int, float>(16);

    public static bool Dunk = true;
    public static Dictionary<int, int> HasBasketBall = new Dictionary<int, int>(16);
    public static Dictionary<int, ArrowList> PlayerArrows = new Dictionary<int, ArrowList>(16);
    public static Dictionary<int, Image> BasketBallImages = new Dictionary<int, Image>(16);

    [HarmonyPatch(typeof(Player), nameof(Player.Added))]
    [HarmonyPostfix]
    public static void ctor_Postfix(Player __instance)
    {
        try 
        {
            if (__instance.Level.Session.MatchSettings.Mode != TowerBall.TowerBallEntry.Modes)
            {
                return;
            }

            HasBasketBall[__instance.PlayerIndex] = 0;
            touchedGroundSinceCollect[__instance.PlayerIndex] = true;
            currentHoldFrames[__instance.PlayerIndex] = 0f;
            BasketBallImages[__instance.PlayerIndex] = new OutlineImage(TFGame.Atlas["Suyooo.TowerBall/towerball/ball"])
            {
                Origin = Vector2.One * 5f
            };
        }
        catch (Exception ex)
        {
            TowerBallModModule.Instance.Logger.LogError("An exception occurs: {ex}", ex);
        }
        //Dunk = !__instance.Level.Session.MatchSettings.Variants.GetCustomVariant("NoDunking");
    }

    [HarmonyPatch(typeof(Player), "ShootArrow")]
    [HarmonyPrefix]
    public static void ShootArrow_Prefix(Player __instance)
    {
        try 
        {
            if (__instance.Level.Session.MatchSettings.Mode != TowerBall.TowerBallEntry.Modes)
            {
                return;
            }

            PlayerArrows[__instance.PlayerIndex] = __instance.Arrows;

            if (HasBasketBall[__instance.PlayerIndex] > 0)
            {
                DynamicData.For(__instance).Set("Arrows", new ArrowList([BasketBall.BasketBallEntry.ArrowTypes]));
                ((TowerBallRoundLogic)__instance.Level.Session.RoundLogic).LastThrower = __instance.PlayerIndex;

                HasBasketBall[__instance.PlayerIndex] = 0;
            }
        }
        catch (Exception ex)
        {
            TowerBallModModule.Instance.Logger.LogError("An exception occurs: {ex}", ex);
        }
    }

    [HarmonyPatch(typeof(Player), "ShootArrow")]
    [HarmonyPostfix]
    public static void ShootArrow_Postfix(Player __instance)
    {
        try 
        {
            if (__instance.Level.Session.MatchSettings.Mode == TowerBall.TowerBallEntry.Modes)
            {
                DynamicData.For(__instance).Set("Arrows", PlayerArrows[__instance.PlayerIndex]);
            }
        }
        catch (Exception ex)
        {
            TowerBallModModule.Instance.Logger.LogError("An exception occurs: {ex}", ex);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.CollectArrows))]
    [HarmonyPrefix]
    public static bool CollectArrows_Prefix(Player __instance, ArrowTypes[] arrows, out bool __result)
    {
        try 
        {
            if (__instance.Level.Session.MatchSettings.Mode != TowerBall.TowerBallEntry.Modes)
            {
                __result = false;
                return true;
            }

            if (arrows != null && arrows.Length == 1 && arrows[0] == BasketBall.BasketBallEntry.ArrowTypes)
            {
                HasBasketBall[__instance.PlayerIndex] = 1;
                touchedGroundSinceCollect[__instance.PlayerIndex] = true;
                __result = true;
                __instance.ArcherData.SFX.ArrowRecover.Play(__instance.X);
                return false;
            }

            __result = false;
            return true;
        }
        catch (Exception ex)
        {
            TowerBallModModule.Instance.Logger.LogError("An exception occurs: {ex}", ex);
            __result = false;
            return true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnExplode))]
    [HarmonyPrefix]
    public static void OnExplode_Prefix(Player __instance)
    {
        try 
        {
            if (__instance.Level.Session.MatchSettings.Mode != TowerBall.TowerBallEntry.Modes)
            {
                return;
            }

            while (HasBasketBall[__instance.PlayerIndex] > 0)
            {
                ((TowerBallRoundLogic)__instance.Level.Session.RoundLogic).DropBall(__instance, __instance.Position, __instance.Facing);
                HasBasketBall[__instance.PlayerIndex] -= 1;
            }
        }
        catch (Exception ex)
        {
            TowerBallModModule.Instance.Logger.LogError("An exception occurs: {ex}", ex);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Die), [typeof(DeathCause), typeof(int), typeof(bool), typeof(bool)])]
    [HarmonyPrefix]
    public static void Die_Prefix(Player __instance, DeathCause deathCause, int killerIndex)
    {
        try 
        {
            if (__instance.Level.Session.MatchSettings.Mode != TowerBall.TowerBallEntry.Modes)
            {
                return;
            }

            var level = __instance.Level;

            while (HasBasketBall[__instance.PlayerIndex] > 0)
            {
                if (deathCause == DeathCause.JumpedOn && level.GetPlayer(killerIndex) != null)
                {
                    HasBasketBall[killerIndex] += 1;
                }
                else 
                {
                    ((TowerBallRoundLogic)__instance.Level.Session.RoundLogic).DropBall(
                    __instance, __instance.Position + Player.ArrowOffset, __instance.Facing);
                }

                HasBasketBall[__instance.PlayerIndex] -= 1;
            }

            if (currentHoldFrames[__instance.PlayerIndex] > 0f)
            {
                currentHoldFrames[__instance.PlayerIndex] = 0f;
            }
        }
        catch (Exception ex)
        {
            TowerBallModModule.Instance.Logger.LogError("An exception occurs: {ex}", ex);
        }
    }

    [HarmonyPatch(typeof(Player), "Update")]
    [HarmonyPostfix]
    public static void Update_Postfix(Player __instance)
    {
        try 
        {
            if (__instance.Level.Session.MatchSettings.Mode != TowerBall.TowerBallEntry.Modes)
            {
                return;
            }

            BasketBallImages[__instance.PlayerIndex].Position = __instance.Position + Player.ArrowOffset + new Vector2((float)__instance.Facing * 4f, 2f);
            if ((__instance.CharacterIndex == 8 && __instance.AltSelect == ArcherData.ArcherTypes.Normal) ||
                (__instance.CharacterIndex == 6 && __instance.AltSelect == ArcherData.ArcherTypes.Alt) ||
                (__instance.CharacterIndex == 7 && __instance.AltSelect == ArcherData.ArcherTypes.Alt))
            {
                DynamicData.For(__instance).Get<Sprite<string>>("bowSprite")!.Visible = HasBasketBall[__instance.PlayerIndex] <= 0 && __instance.Aiming;
            }
            else
            {
                DynamicData.For(__instance).Get<Sprite<string>>("bowSprite")!.Visible = HasBasketBall[__instance.PlayerIndex] <= 0;
            }

            if (__instance.State == Player.PlayerStates.LedgeGrab)
            {
                touchedGroundSinceCollect[__instance.PlayerIndex] = false;
            }
            else 
            {
                touchedGroundSinceCollect[__instance.PlayerIndex] &= !__instance.OnGround;
            }

            if (HasBasketBall[__instance.PlayerIndex] > 0)
            {
                currentHoldFrames[__instance.PlayerIndex] += Engine.TimeMult;
            }
            else if (currentHoldFrames[__instance.PlayerIndex] > 0f)
            {
                currentHoldFrames[__instance.PlayerIndex] = 0f;
            }
        }
        catch (Exception ex)
        {
            TowerBallModModule.Instance.Logger.LogError("An exception occurs: {ex}", ex);
        }
        if (__instance.Level.Session.MatchSettings.Mode != TowerBall.TowerBallEntry.Modes)
        {
            return;
        }

    }

    [HarmonyPatch(typeof(Player), "DuckingUpdate")]
    [HarmonyPrefix]
    public static void DuckingUpdate_Prefix(Player __instance, ref bool __state)
    {
        try 
        {
            if (__instance.Level.Session.MatchSettings.Mode != TowerBall.TowerBallEntry.Modes)
            {
                return;
            }

            bool dunked = false;
            __state = dunked;

            if (HasBasketBall[__instance.PlayerIndex] > 0 && 
                TFGame.PlayerInputs[__instance.PlayerIndex].GetState().JumpPressed &&
                __instance.CollideCheck(GameTags.JumpThru, __instance.Position + Vector2.UnitY) &&
                !__instance.CollideCheck(GameTags.Solid, __instance.Position + Vector2.UnitY * 3f))
            {
                var entity = __instance.CollideFirst(GameTags.JumpThru, __instance.Position + Vector2.UnitY);
                if (entity is BasketBallBasket basket && Dunk)
                {
                    var isRight = __instance.Position.X > (160f + TowerBallModModule.TestWideUI);

                    Sounds.sfx_devTimeFinalDummy.Play(__instance.X);
                    var roundLogic = (TowerBallRoundLogic)__instance.Level.Session.RoundLogic;

                    while (HasBasketBall[__instance.PlayerIndex] > 0)
                    {
                        roundLogic.IncreaseScore(__instance.PlayerIndex, roundLogic.LastThrower, dunk: true, allyoop: false, clean: false, !isRight ? 1 : 0);
                        roundLogic.AddDeadBall(__instance.Position, __instance.Speed);
                        HasBasketBall[__instance.PlayerIndex] -= 1;
                    }

                    roundLogic.SpawnBallChest(300);
                    Explosion.Spawn(
                        __instance.Level, 
                        entity.Position + new Vector2(7.5f, 0f), 
                        __instance.PlayerIndex, 
                        plusOneKill: false, triggerBomb: false, bombTrap: false);
                    roundLogic.AddSlamNotification(entity.Position + (isRight ? new Vector2(-10f, 0f) : new Vector2(10f, 0)));
                    if (__instance.Level.KingIntro)
                    {
                        __instance.Level.KingIntro.Laugh();
                    }

                    basket.DoNetJump();
                    dunked = true;
                }
            }

            __state = dunked;
        }
        catch (Exception ex)
        {
            TowerBallModModule.Instance.Logger.LogError("An exception occurs: {ex}", ex);
        }
    }

    [HarmonyPatch(typeof(Player), "DuckingUpdate")]
    [HarmonyPostfix]
    public static void DuckingUpdate_Postfix(Player __instance, ref bool __state)
    {
        if (__state)
        {
            __instance.Position.Y += 6f;
        }

        if (__state && __instance.HasShield)
        {
            __instance.Position.Y += 10f;
        }
    }

    public static void HUDRender_Postfix(LevelEntity __instance)
    {
        try 
        {
            if (__instance.Level is not { } level)
            {
                return;
            }

            if (level.Session.MatchSettings.Mode != TowerBall.TowerBallEntry.Modes)
            {
                return;
            }

            if (__instance is not Player player)
            {
                return;
            }

            if (HasBasketBall[player.PlayerIndex] > 0)
            {
                BasketBallImages[player.PlayerIndex].Render();
            }
        }
        catch (Exception ex)
        {
            TowerBallModModule.Instance.Logger.LogError("An exception occurs: {ex}", ex);
        }
    }

    public static void Patch(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LevelEntity), nameof(LevelEntity.Render)),
            postfix: new HarmonyMethod(HUDRender_Postfix)
        );
    }
}

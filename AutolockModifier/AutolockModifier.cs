using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.AutolockModifier;

public sealed class AutolockModifier : Mod
{
    public static AutolockModifier Instance { get; private set; } = null!;
    public AutolockModifierSettings Settings => GetSettings<AutolockModifierSettings>()!;

    private static IVariantEntry noAutolock = null!;
    private static IVariantEntry smartAutolock = null!;

    public AutolockModifier(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;

        smartAutolock = context.Registry.Variants.RegisterVariant(
            "SmartAutolock",
            new() 
            {
                Title = "SMART AUTOLOCK",
                Icon = context.Registry.Subtextures.RegisterTexture(
                    content.Root.GetRelativePath("Content/variants/smartAutolock.png")),
                Description = "PREDICTS THE TARGET BASED ON ITS VELOCITY"
            }
        );
        
        noAutolock = context.Registry.Variants.RegisterVariant(
            "NoAutolock",
            new() 
            {
                Title = "NO AUTOLOCK",
                Icon = context.Registry.Subtextures.RegisterTexture(
                    content.Root.GetRelativePath("Content/variants/noAutolock.png")),
                Links = [smartAutolock]
            }
        );


        context.Harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), "FindAutoLockAngle"),
            prefix: new HarmonyMethod(Player_FindAutoLockAngle_Prefix, priority: -400),
            transpiler: new HarmonyMethod(Player_FindAutoLockAngle_Transpiler, priority: -400)
        );
    }

    private static IEnumerable<CodeInstruction> Player_FindAutoLockAngle_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext([
            ILMatch.Ldloc().TryGetLocalIndex(out var vector), 
            ILMatch.Ldloc().TryGetLocalIndex(out var levelEntity2), 
            ILMatch.Ldfld("Position"), 
            ILMatch.CallOrCallvirt("DistanceSquared"), 
            ILMatch.Stloc().TryGetLocalIndex(out var num3)]);

        cursor.GotoNext(MoveType.After, ILMatch.LdcR4(1296));
        cursor.Emit(new CodeInstruction(OpCodes.Ldarg_0));
        cursor.EmitDelegate(static (float x, Player player) => {
            if (player.Level.Session.MatchSettings.Mode == Modes.Trials && !Instance.Settings.AllowTrials)
            {
                return x;
            }

            return Instance.Settings.MaxDistanceInPixels * Instance.Settings.MaxDistanceInPixels;
        });

        cursor.GotoNext(MoveType.After, ILMatch.Ldfld("Position"));
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldloca, vector.Value);
        cursor.Emit(OpCodes.Ldloc, levelEntity2.Value);
        cursor.Emit(OpCodes.Ldloca, num3.Value);
        cursor.EmitDelegate(static (Vector2 targetPosition, Player p, in Vector2 vector, LevelEntity levelEntity2, in float num3) => 
        {
            if (!smartAutolock.IsActive() && (Instance.Settings.AutolockBehavior != "Smart" 
                || (levelEntity2.Level.Session.MatchSettings.Mode == Modes.Trials 
                    && !Instance.Settings.AllowTrials)))
            {
                return targetPosition;
            }

            Vector2 speed;

            if (levelEntity2 is Player)
            {
                speed = p.Speed;
            }
            else if (levelEntity2 is Enemy enemy)
            {
                speed = enemy.Speed;
            }
            else 
            {
                return targetPosition;
            }

            Vector2 target = targetPosition;

            const float ArrowSpeed = 4f;

            float dist2 = num3;
            float t = 0;

            //Console.WriteLine("INITIAL TARGET: " + target);

            for (int i = 0; i < 5; i += 1)
            {
                dist2 = Vector2.DistanceSquared(vector, target);
                t = (float)Math.Sqrt(dist2) / ArrowSpeed;
                target = new Vector2(
                    levelEntity2.Position.X + speed.X * t, 
                    levelEntity2.Position.Y + speed.Y * t);

                //Console.WriteLine("PREDICTION TARGET: " + target);
            }

            //Console.WriteLine("FINAL TARGET: " + target);
            //Console.WriteLine("FINAL ANGLE: " + (levelEntity2.Position - vector).Angle() * Calc.RAD_TO_DEG);
            //Console.WriteLine("AIM DIRECTION: " + p.AimDirection * Calc.RAD_TO_DEG);
            //Console.WriteLine("PLAYER POSITION: " + p.Speed);

            return target + levelEntity2.SeekOffset;
        });

        cursor.GotoNext(MoveType.After, ILMatch.LdcR4(1.134464f));
        cursor.Emit(new CodeInstruction(OpCodes.Ldarg_0));
        cursor.EmitDelegate((float x, Player player) => {
            if (player.Level.Session.MatchSettings.Mode == Modes.Trials && !Instance.Settings.AllowTrials)
            {
                return x;
            }
            return Instance.Settings.MaxAngle == 65 ? x : Instance.Settings.MaxAngle * Calc.DEG_TO_RAD;
        });

        return cursor.Generate();
    }

    private static bool Player_FindAutoLockAngle_Prefix(Player __instance, ref float __result)
    {
        if (noAutolock.IsActive())
        {
            __result = __instance.AimDirection;
            return false;
        }

        if (__instance.Level.Session.MatchSettings.Mode == Modes.Trials && !Instance.Settings.AllowTrials)
        {
            return true;
        }

        if (Instance.Settings.DisableAutoLock)
        {
            __result = __instance.AimDirection;
            return false;
        }
        return true;
    }

    public override ModuleSettings? CreateSettings()
    {
        return new AutolockModifierSettings();
    }
}

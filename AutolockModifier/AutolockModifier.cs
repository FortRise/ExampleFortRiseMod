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

    public AutolockModifier(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;
        context.Harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), "FindAutoLockAngle"),
            prefix: new HarmonyMethod(Player_FindAutoLockAngle_Prefix, priority: -400),
            transpiler: new HarmonyMethod(Player_FindAutoLockAngle_Transpiler, priority: -400)
        );
    }

    private static IEnumerable<CodeInstruction> Player_FindAutoLockAngle_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, ILMatch.LdcR4(1296));
        cursor.Emit(new CodeInstruction(OpCodes.Ldarg_0));
        cursor.EmitDelegate((float x, Player player) => {
            if (player.Level.Session.MatchSettings.Mode == Modes.Trials && !Instance.Settings.AllowTrials)
            {
                return x;
            }

            return Instance.Settings.MaxDistanceInPixels * Instance.Settings.MaxDistanceInPixels;
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

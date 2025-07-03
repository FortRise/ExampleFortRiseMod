using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FortRise;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class UnsafeVersusLevelSystem
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "levels")]
    public static extern ref List<string> GetLevels(VersusLevelSystem instance);
}

internal sealed class VersusLevelSystemHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(VersusLevelSystem), "GenLevels"),
            postfix: new HarmonyMethod(VersusLevelSystem_GenLevels_Postfix)
        );
    }

    private static void VersusLevelSystem_GenLevels_Postfix(VersusLevelSystem __instance)
    {
        if (!WiderSetModule.IsWide)
        {
            return;
        }

        ref var levels = ref UnsafeVersusLevelSystem.GetLevels(__instance);
        List<string> wideLevels = new List<string>(levels.Count);
        foreach (var level in levels)
        {
            // if the first level starts with 'mod:'
            // it means this tower is modded
            if (level.StartsWith("mod:"))
            {
                break;
            }

            // redirect the level to us instead
            string prefix = "mod:Teuria.WiderSet/";
            string actualLevel = $"{prefix}{level}".Replace("Levels", "WideLevels");
            wideLevels.Add(actualLevel);
        }

        levels = wideLevels;
    }
}
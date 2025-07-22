using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Monocle;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class MatchTeamsHooks : IHookable
{
    private static Dictionary<int, Allegiance> moreAllegiances = new Dictionary<int, Allegiance>();

    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MatchTeams), "get_Item"),
            finalizer: new HarmonyMethod(MatchTeams_get_Item_Finalizer)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MatchTeams), "set_Item"),
            finalizer: new HarmonyMethod(MatchTeams_set_Item_Finalizer)
        );

        harmony.Patch(
            AccessTools.DeclaredPropertyGetter(typeof(MatchTeams), nameof(MatchTeams.ProperlyAssigned)),
            transpiler: new HarmonyMethod(MatchTeams_ProperlyAssigned_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredPropertyGetter(typeof(MatchTeams), nameof(MatchTeams.HasEvenTeams)),
            new HarmonyMethod(MatchTeams_HasEvenTeams_Prefix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MatchTeams), nameof(MatchTeams.GetAmountOfPlayersOfAllegiance)),
            postfix: new HarmonyMethod(MatchTeams_GetAmountOfPlayersOfAllegiance_Postfix)
        ); 

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MatchTeams), nameof(MatchTeams.GetPlayersOfAllegiance)),
            postfix: new HarmonyMethod(MatchTeams_GetPlayersOfAllegiance_Postfix)
        ); 
    }

    private static bool MatchTeams_HasEvenTeams_Prefix(MatchTeams __instance, ref bool __result)
    {
        if (WiderSetModule.IsWide)
        {
            var player1Team = Private.Field<MatchTeams, Allegiance>("player1Team", __instance);
            var player2Team = Private.Field<MatchTeams, Allegiance>("player2Team", __instance);
            var player3Team = Private.Field<MatchTeams, Allegiance>("player3Team", __instance);
            var player4Team = Private.Field<MatchTeams, Allegiance>("player4Team", __instance);
            int blue = Calc.Count(
                Allegiance.Blue,
                player1Team.Read(),
                player2Team.Read(),
                player3Team.Read(),
                player4Team.Read(), 
                GetExtraAllegianceByIndex(4),
                GetExtraAllegianceByIndex(5),
                GetExtraAllegianceByIndex(6),
                GetExtraAllegianceByIndex(7)
            );

            int red = Calc.Count(
                Allegiance.Red,
                player1Team.Read(),
                player2Team.Read(),
                player3Team.Read(),
                player4Team.Read(), 
                GetExtraAllegianceByIndex(4),
                GetExtraAllegianceByIndex(5),
                GetExtraAllegianceByIndex(6),
                GetExtraAllegianceByIndex(7)
            );

            __result = blue == red;
            return false;
        }
        return true;
    }

    private static Allegiance GetExtraAllegianceByIndex(int index)
    {
        ref var all = ref CollectionsMarshal.GetValueRefOrNullRef(moreAllegiances, index);
        if (Unsafe.IsNullRef(ref all))
        {
            return Allegiance.Neutral;
        }

        return all;
    }

    private static void MatchTeams_GetPlayersOfAllegiance_Postfix(Allegiance allegiance, ref List<int> __result)
    {
        if (!WiderSetModule.IsWide)
        {
            return;
        }

        if (IsSameAllegianceAndExist(4, allegiance))
        {
            __result.Add(4);
        }
        if (IsSameAllegianceAndExist(5, allegiance))
        {
            __result.Add(5);
        }
        if (IsSameAllegianceAndExist(6, allegiance))
        {
            __result.Add(6);
        }
        if (IsSameAllegianceAndExist(7, allegiance))
        {
            __result.Add(7);
        }
    }

    private static void MatchTeams_GetAmountOfPlayersOfAllegiance_Postfix(Allegiance allegiance, ref int __result)
    {
        if (!WiderSetModule.IsWide)
        {
            return;
        }

        if (IsSameAllegianceAndExist(4, allegiance))
        {
            __result += 1;
        }
        if (IsSameAllegianceAndExist(5, allegiance))
        {
            __result += 1;
        }
        if (IsSameAllegianceAndExist(6, allegiance))
        {
            __result += 1;
        }
        if (IsSameAllegianceAndExist(7, allegiance))
        {
            __result += 1;
        }
    }

    private static bool IsSameAllegianceAndExist(int index, Allegiance allegiance)
    {
        ref var all = ref CollectionsMarshal.GetValueRefOrNullRef(moreAllegiances, index);
        if (Unsafe.IsNullRef(ref all))
        {
            return false;
        }

        return TFGame.Players[index] && allegiance == all;
    }

    private static IEnumerable<CodeInstruction> MatchTeams_ProperlyAssigned_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcI4(4)
            ]
        );

        cursor.EmitDelegate((int player) =>
        {
            if (WiderSetModule.IsWide)
            {
                return player + 4;
            }

            return player;
        });

        return cursor.Generate();
    }

    private static Exception? MatchTeams_get_Item_Finalizer(int index, ref Allegiance __result)
    {
        ref var allegianceRef = ref CollectionsMarshal.GetValueRefOrAddDefault(moreAllegiances, index, out bool exists);

        if (!exists)
        {
            allegianceRef = Allegiance.Neutral;
        }

        __result = allegianceRef;

        return null; // supress exception
    }

    private static Exception? MatchTeams_set_Item_Finalizer(int index, Allegiance value)
    {
        moreAllegiances[index] = value;
        return null; // supress exception
    }
}

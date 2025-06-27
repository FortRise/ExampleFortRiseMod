using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class UnfairAutobalance : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Session), nameof(Session.GetSpawnShield)),
            transpiler: new HarmonyMethod(Session_GetSpawnShield_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Session), "GetSpawnArrows"),
            transpiler: new HarmonyMethod(Session_GetSpawnArrows_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> Session_GetSpawnArrows_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcI4(6)
            ]
        );

        cursor.Emit(new CodeInstruction(OpCodes.Ldarg_0));
        cursor.EmitDelegate((int arrows, Session session) =>
        {
            if (Variants.UnfairAutobalance.IsActive() && HasOnePlayerCrown(session))
            {
                return 4;
            }

            return arrows;
        });

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcI4(4)
            ]
        );

        cursor.EmitDelegate((int arrows) =>
        {
            if (Variants.UnfairAutobalance.IsActive())
            {
                return 6;
            }

            return arrows;
        });

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcI4(3)
            ]
        );

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((int arrows, Session session) =>
        {
            if (Variants.UnfairAutobalance.IsActive() && HasOnePlayerCrown(session))
            {
                return 2;
            }

            return arrows;
        });

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Call("Max")
            ]
        );

        cursor.EmitDelegate((int arrows) =>
        {
            if (Variants.UnfairAutobalance.IsActive())
            {
                return 3;
            }

            return arrows;
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> Session_GetSpawnShield_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Cgt()
            ]
        );

        cursor.Emits([
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldarg_1),
        ]);

        cursor.EmitDelegate((bool isLosing, Session session, int playerIndex) =>
        {
            if (Variants.UnfairAutobalance.IsActive() && HasOnePlayerCrown(session))
            {
                if (session.GetSpawnHatState(playerIndex) == Player.HatStates.Crown)
                {
                    return false;
                }

                return true;
            }

            return isLosing;
        });

        return cursor.Generate();
    }

    private static bool HasOnePlayerCrown(Session session)
    {
        var amount = TFGame.PlayerAmount;

        for (int i = 0; i < amount; i++)
        {
            if (session.GetSpawnHatState(i) == Player.HatStates.Crown)
            {
                return true;
            }
        }

        return false;
    }
}
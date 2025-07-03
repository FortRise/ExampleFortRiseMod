using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class ActorHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Actor), nameof(Actor.MoveTowardsWrap)),
            transpiler: new HarmonyMethod(Actor_MoveTowardsWrap_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> Actor_MoveTowardsWrap_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcR4(320f)
            ]
        );

        cursor.EmitDelegate((float x) => {
            if (WiderSetModule.IsWide)
            {
                return x + 100f;
            }
            return x;
        });

        return cursor.Generate();
    }
}

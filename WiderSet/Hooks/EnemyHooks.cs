using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class EnemyHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Enemy), nameof(Enemy.GetHorizontalPlayer), [typeof(Vector2), typeof(Facing)]),
            transpiler: new HarmonyMethod(Enemy_GetHorizontalPlayer_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> Enemy_GetHorizontalPlayer_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
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

using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class LevelBGTilesHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LevelBGTiles), nameof(LevelBGTiles.Added)),
            transpiler: new HarmonyMethod(LevelBGTiles_Added_HandleGraphicsDispose_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LevelBGTiles), nameof(LevelBGTiles.HandleGraphicsDispose)),
            transpiler: new HarmonyMethod(LevelBGTiles_Added_HandleGraphicsDispose_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> LevelBGTiles_Added_HandleGraphicsDispose_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcI4(320)
            ]
        );

        cursor.EmitDelegate((int x) => {
            if (WiderSetModule.IsWide)
            {
                return x + 100;
            }
            return x;
        });

        return cursor.Generate();
    }
}
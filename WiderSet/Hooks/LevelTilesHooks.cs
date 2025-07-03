using System.Collections.Generic;
using System.Reflection.Emit;
using System.Xml;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class LevelTilesHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(LevelTiles), [typeof(XmlElement), typeof(bool[,])]),
            transpiler: new HarmonyMethod(LevelTiles_ctor_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LevelTiles), nameof(LevelTiles.Added)),
            transpiler: new HarmonyMethod(LevelTiles_Added_HandleGraphicsDispose_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LevelTiles), nameof(LevelTiles.HandleGraphicsDispose)),
            transpiler: new HarmonyMethod(LevelTiles_Added_HandleGraphicsDispose_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> LevelTiles_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcI4(32)
            ]
        );

        cursor.EmitDelegate((int x) => {
            if (WiderSetModule.IsWide)
            {
                return x + 10;
            }
            return x;
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> LevelTiles_Added_HandleGraphicsDispose_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
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

using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall.Editor;

namespace Teuria.WiderSet;

internal sealed class EditorBaseHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(EditorBase), "InitEditorScreen"),
            new HarmonyMethod(EditorBase_InitEditorScreen_Prefix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(EditorBase), "ReturnToGameScreen"),
            new HarmonyMethod(EditorBase_ReturnToGameScreen_Prefix),
            transpiler: new HarmonyMethod(EditorBase_ReturnToGameScreen_Transpiler)
        );
    }

    private static void EditorBase_InitEditorScreen_Prefix()
    {
        WiderSetModule.InsideOfTheEditor = true;
    }

    private static void EditorBase_ReturnToGameScreen_Prefix()
    {
        WiderSetModule.InsideOfTheEditor = false;
    }

    private static IEnumerable<CodeInstruction> EditorBase_ReturnToGameScreen_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcI4(320)
            ]
        );

        cursor.EmitDelegate((int width) => width + 100);

        return cursor.Generate();
    }
}

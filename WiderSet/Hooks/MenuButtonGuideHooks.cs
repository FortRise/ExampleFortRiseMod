using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Monocle;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class MenuButtonGuideHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(MenuButtonGuide), [typeof(int), typeof(MenuButtonGuide.ButtonModes), typeof(string)]),
            transpiler: new HarmonyMethod(MenuButtonGuide_ctor_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> MenuButtonGuide_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcR4(310f)]);

        cursor.EmitDelegate((float width) =>
        {
            if (WiderSetModule.IsWide && Engine.Instance.Scene is Level)
            {
                return width + 50;
            }

            return width;
        });

        return cursor.Generate();
    }
}
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Xml;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class WrapHitboxHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(WrapHitbox), "BuildHitList"),
            transpiler: new HarmonyMethod(WrapHitbox_BuildHitList_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> WrapHitbox_BuildHitList_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcR4(320f)
            ]
        );

        cursor.EmitDelegate((float width) =>
        {
            if (WiderSetModule.IsWide)
            {
                return width + 100f;
            }

            return width;
        });

        return cursor.Generate();
    }
}

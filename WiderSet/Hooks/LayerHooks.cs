using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;

namespace Teuria.WiderSet;

internal sealed class LayerHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Layer), nameof(Layer.Render)),
            transpiler: new HarmonyMethod(Layer_Render_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> Layer_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Call("get_Identity")
            ]
        );

        cursor.EmitDelegate((Matrix x) =>
        {
            return MatrixUtilities.IdentityFixed;
        });

        return cursor.Generate();
    }
}
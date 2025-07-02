using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class LevelHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Level), nameof(Level.CoreRender)),
            transpiler: new HarmonyMethod(Level_CoreRender_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> Level_CoreRender_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Ldarg(1),
                ILMatch.Callvirt("SetRenderTarget")
            ]
        );

        cursor.EmitDelegate(() => Engine.Instance.GraphicsDevice.Clear(Color.Transparent));

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Call("get_Zero")
            ]
        );

        cursor.EmitDelegate((Vector2 x) => x + new Vector2(Screen.LeftImage.Width - 2, 0));


        return cursor.Generate();
    }
}

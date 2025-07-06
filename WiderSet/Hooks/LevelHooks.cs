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
            AccessTools.DeclaredMethod(typeof(Level), nameof(Level.orig_ctor)),
            transpiler: new HarmonyMethod(Level_ctor_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Level), nameof(Level.DebugModeRender)),
            transpiler: new HarmonyMethod(Level_DebugModeRender_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Level), nameof(Level.CoreRender)),
            transpiler: new HarmonyMethod(Level_CoreRender_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> Level_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcI4(320),
            ]
        );

        cursor.EmitDelegate((int width) =>
        {
            if (WiderSetModule.IsWide)
            {
                return width + 100;
            }
            return width;
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> Level_DebugModeRender_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Call("Lerp")
            ]
        );

        cursor.EmitDelegate((Matrix x) =>
        {
            if (WiderSetModule.IsWide) { return x; }
            Engine.Instance.Scene.Camera.X -= Screen.LeftImage.Width - 2;
            var matrix = Engine.Instance.Scene.Camera.Matrix;
            Engine.Instance.Scene.Camera.X += Screen.LeftImage.Width - 2;
            return matrix;
        });


        return cursor.Generate();
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

        cursor.EmitDelegate((Vector2 x) => {
            if (!WiderSetModule.IsWide)
            {
                return x + new Vector2(Screen.LeftImage.Width - 2, 0);
            }
            return x;
        });


        return cursor.Generate();
    }
}

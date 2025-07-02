using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class BackgroundHooks : IHookable
{
    private static RasterizerState ScissorRasterizer = null!;

    public static void Load(IHarmony harmony)
    {
        ScissorRasterizer = new RasterizerState
        {
            CullMode = CullMode.None,
            ScissorTestEnable = true
        };

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Background), nameof(Background.Render)),
            transpiler: new HarmonyMethod(Background_Render_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> Background_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Ldsfld("CullNone")
            ]
        );

        cursor.EmitDelegate((RasterizerState state) =>
        {
            if (WiderSetModule.IsWide)
            {
                return state;
            }
            return ScissorRasterizer;
        });

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Call("get_Identity")
            ]
        );

        cursor.EmitDelegate((Matrix x) =>
        {
            Engine.Instance.Scene.Camera.X -= Screen.LeftImage.Width;
            var matrix = Engine.Instance.Scene.Camera.Matrix;
            Engine.Instance.Scene.Camera.X += Screen.LeftImage.Width;
            return matrix;
        });


        cursor.GotoNext(MoveType.After, [ILMatch.Callvirt("Begin")]);

        cursor.EmitDelegate(() =>
        {
            if (!WiderSetModule.IsWide)
            {
                Engine.Instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, 320, 240);
            }
        });

        return cursor.Generate();
    }
}
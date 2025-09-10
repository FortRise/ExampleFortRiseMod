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

internal sealed class IntroSceneHooks : IHookable
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
            AccessTools.DeclaredMethod(typeof(IntroScene), nameof(IntroScene.Render)),
            transpiler: new HarmonyMethod(IntroScene_Render_Transpiler)
        );

        harmony.Patch(
            AccessTools.EnumeratorMoveNext(
                AccessTools.DeclaredMethod(typeof(IntroScene), "Sequence")
            ),
            transpiler: new HarmonyMethod(IntroScene_Sequence_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> IntroScene_Sequence_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x => 
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(160f)]))
            {
                cursor.EmitDelegate((float width) => width + 50);
            }
        });

        cursor.Index = 0;

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(160)]);
        cursor.EmitDelegate((int width) => width + 50);

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> IntroScene_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
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

        cursor.GotoNext(MoveType.After, [ILMatch.Callvirt("Begin")]);
        cursor.EmitDelegate(() =>
        {
            ref var matrix = ref UnsafeSpriteBatch.GetSpriteBatchTransformMatrix(Draw.SpriteBatch);
            matrix = MatrixUtilities.IdentityFixed;

            Engine.Instance.GraphicsDevice.ScissorRectangle = new Rectangle(
                55,
                0,
                420 - 110,
                240
            );
        });

        return cursor.Generate();
    }
}



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

internal sealed class ReplayFrameHooks : IHookable
{
    private static Matrix Matrix;
    public static void Load(IHarmony harmony)
    {
        Matrix = Matrix.Identity *
            Matrix.CreateTranslation(new Vector3(-new Vector2(54, 0), 0f)) *
            Matrix.CreateRotationZ(0f) *
            Matrix.CreateScale(new Vector3(Vector2.One, 1f)) *
            Matrix.CreateTranslation(new Vector3(new Vector2(0, 0), 0f));

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(ReplayFrame), nameof(ReplayFrame.Record)),
            transpiler: new HarmonyMethod(ReplayFrame_Record_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> ReplayFrame_Record_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcI4(76800)
            ]
        );

        cursor.EmitDelegate((int size) =>
        {
            if (WiderSetModule.IsWide)
            {
                return 420 * 240;
            }

            return size;
        });

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcI4(320)
            ]
        );

        cursor.EmitDelegate((int size) =>
        {
            if (WiderSetModule.IsWide)
            {
                return size + 100;
            }

            return size;
        });

        cursor.GotoNext(MoveType.After, [ILMatch.Ldsfld("Opaque")]);
        cursor.EmitDelegate((BlendState _) => BlendState.AlphaBlend);

        cursor.GotoNext(MoveType.After, [ILMatch.Callvirt("Begin")]);

        cursor.EmitDelegate(() =>
        {
            if (WiderSetModule.IsWide)
            {
                return;
            }

            ref var matrix = ref UnsafeSpriteBatch.GetSpriteBatchTransformMatrix(Draw.SpriteBatch);
            matrix = Matrix;
        });

        cursor.Encompass((x) =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(4)]))
            {
                cursor.EmitDelegate((int player) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return player + 4;
                    }

                    return player;
                });
            }
        });

        return cursor.Generate();
    }
}
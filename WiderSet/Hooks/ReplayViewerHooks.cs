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

internal sealed class ReplayViewerHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(ReplayViewer)),
            transpiler: new HarmonyMethod(ReplayViewer_ctor_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(ReplayViewer), nameof(ReplayViewer.Render)),
            prefix: new HarmonyMethod(ReplayViewer_Render_Prefix),
            transpiler: new HarmonyMethod(ReplayViewer_Render_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(ReplayViewer), nameof(ReplayViewer.PostScreenRender)),
            transpiler: new HarmonyMethod(ReplayViewer_PostScreenRender_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> ReplayViewer_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(320)]);

        cursor.EmitDelegate((int width) =>
        {
            if (WiderSetModule.IsWide)
            {
                return width + 100;
            }

            return width;
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

    private static void ReplayViewer_Render_Prefix()
    {
        Draw.Clear(Color.Transparent);
    }

    private static IEnumerable<CodeInstruction> ReplayViewer_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

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
            matrix = MatrixUtilities.IdentityFixed;
        });

        cursor.GotoNext(MoveType.After, [ILMatch.LdcR4(320f)]);

        cursor.EmitDelegate((float width) =>
        {
            if (WiderSetModule.IsWide)
            {
                return width + 100;
            }

            return width;
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> ReplayViewer_PostScreenRender_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass((x) =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(310f)]))
            {
                cursor.EmitDelegate((float width) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return width + 50;
                    }

                    return width;
                });
            }
        });

        return cursor.Generate();
    }
}

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Teuria.WiderSet;

internal static class UnsafeScreen
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "viewport")]
    public static extern ref Viewport GetViewport(Screen screen);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "rightPadDrawRect")]
    public static extern ref Rectangle RightDrawPadRect(Screen screen);
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "leftPadDrawRect")]
    public static extern ref Rectangle LeftDrawPadRect(Screen screen);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "UpdatePadRects")]
    public static extern void UpdatePadRects(Screen screen);
}

internal sealed class ScreenHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Screen), nameof(Screen.DisableFullscreen), [typeof(float)]),
            postfix: new HarmonyMethod(Screen_DisableFullscreen_Postfix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Screen), nameof(Screen.EnableFullscreen), [typeof(Screen.FullscreenMode)]),
            postfix: new HarmonyMethod(Screen_EnableFullscreen_Postfix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Screen), nameof(Screen.Render)),
            transpiler: new HarmonyMethod(Screen_Render_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> Screen_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.Ldsfld("Opaque")]);

        cursor.EmitDelegate((BlendState _) => BlendState.AlphaBlend);

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(320f)]))
            {
                cursor.EmitDelegate((float width) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return width + 100;
                    }

                    return width;
                });
            }
        });

        cursor.Index = 0;

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.Callvirt("get_Width")]))
            {
                cursor.EmitDelegate((int width) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return width;
                    }

                    return width - 100;
                });
            }
        });

        return cursor.Generate();
    }

    private static void Screen_EnableFullscreen_Postfix(Screen __instance)
    {
        var offset = Screen.LeftImage.Width * 3;
        UnsafeScreen.UpdatePadRects(__instance);
        ref var rect = ref UnsafeScreen.RightDrawPadRect(__instance);
        rect.X -= 209;
        ref var leftRect = ref UnsafeScreen.LeftDrawPadRect(__instance);
        leftRect.X += offset + 80;
    }

    private static void Screen_DisableFullscreen_Postfix(Screen __instance)
    {
        var offset = Screen.LeftImage.Width * 3;
        UnsafeScreen.UpdatePadRects(__instance);
        ref var rect = ref UnsafeScreen.RightDrawPadRect(__instance);
        rect.X -= 148;
        ref var leftRect = ref UnsafeScreen.LeftDrawPadRect(__instance);
        leftRect.X += offset - 12;
        __instance.Matrix =
            Matrix.CreateScale(__instance.Scale) *
            Matrix.CreateTranslation(offset, 0f, 0f);
    }
}
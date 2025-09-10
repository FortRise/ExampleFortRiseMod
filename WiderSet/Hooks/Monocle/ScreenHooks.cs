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
            AccessTools.DeclaredMethod(typeof(Screen), "SetWindowSize", [typeof(int), typeof(int), typeof(bool)]),
            postfix: new HarmonyMethod(Screen_SetWindowSize_Postfix),
            transpiler: new HarmonyMethod(Screen_SetWindowSize_Transpiler)
        );

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

    private static IEnumerable<CodeInstruction> Screen_SetWindowSize_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.Ldarg(1)]);

        cursor.Emit(new CodeInstruction(OpCodes.Ldarg_0));
        cursor.EmitDelegate((int width, Screen __instance) => 
        {
            int x = (int)(width + 10 * Private.Field<Screen, float>("scale", __instance).Read());
            return x;
        });

        return cursor.Generate();
    }

    private static void Screen_SetWindowSize_Postfix(int width, Screen __instance)
    {
        float scale = Private.Field<Screen, float>("scale", __instance).Read();
        int x = (int)(width + 10 * scale);
        var viewportPtr = Private.Field<Screen, Viewport>("viewport", __instance);
        var viewport = viewportPtr.Read();
        viewport.Width = x;
        viewportPtr.Write(viewport);

        __instance.DrawRect.X = viewport.Width / 2 - __instance.ScaledWidth / 2;
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
        int x = Private.Field<Screen, Rectangle>("leftPadDrawRect", __instance).Read().Width;
        var offset = (float)(Screen.LeftImage.Width * __instance.Scale);
        var offsetForMatrix = (float)((Screen.LeftImage.Width - 1.6f) * __instance.Scale);
        UnsafeScreen.UpdatePadRects(__instance);

        ref var rect = ref UnsafeScreen.RightDrawPadRect(__instance);
        rect.X = __instance.DrawRect.X + __instance.DrawRect.Width + (int)(5 * Private.Field<Screen, float>("scale", __instance).Read());
        rect.X -= x;

        ref var leftRect = ref UnsafeScreen.LeftDrawPadRect(__instance);
        leftRect.X += (int)(offset - (5 * Private.Field<Screen, float>("scale", __instance).Read()));

        __instance.Matrix =
            Matrix.CreateScale(__instance.Scale) *
            Matrix.CreateTranslation(offsetForMatrix, 0f, 0f);
    }

    private static void Screen_DisableFullscreen_Postfix(Screen __instance)
    {
        int x = Private.Field<Screen, Rectangle>("leftPadDrawRect", __instance).Read().Width;
        var offset = (int)(Screen.LeftImage.Width * __instance.Scale);
        UnsafeScreen.UpdatePadRects(__instance);

        ref var rect = ref UnsafeScreen.RightDrawPadRect(__instance);
        rect.X = __instance.DrawRect.X + __instance.DrawRect.Width + (int)(5 * Private.Field<Screen, float>("scale", __instance).Read());
        rect.X -= x;

        ref var leftRect = ref UnsafeScreen.LeftDrawPadRect(__instance);
        leftRect.X += offset - (int)(5 * Private.Field<Screen, float>("scale", __instance).Read());

        __instance.Matrix =
            Matrix.CreateScale(__instance.Scale) *
            Matrix.CreateTranslation(offset, 0f, 0f);
    }
}

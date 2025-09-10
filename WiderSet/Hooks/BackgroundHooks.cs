using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Xml;
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
    public static Background ForegroundCheck = null!;

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

        ScrollLayerHooks.Load(harmony);
        LightningFlashLayerHooks.Load(harmony);
        VortexLayerHooks.Load(harmony);
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

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((Matrix x, Background __instance) =>
        {
            if (WiderSetModule.IsWide) { return x; }
            if (__instance == ForegroundCheck) { return x; } // do not change anything if its a foreground.
            Engine.Instance.Scene.Camera.X -= Screen.LeftImage.Width - 5;
            var matrix = Engine.Instance.Scene.Camera.Matrix;
            Engine.Instance.Scene.Camera.X += Screen.LeftImage.Width - 5;
            return matrix;
        });


        cursor.GotoNext(MoveType.After, [ILMatch.Callvirt("Begin")]);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((Background __instance) =>
        {
            if (__instance == ForegroundCheck) { return; } // do not change anything if its a foreground.
            if (!WiderSetModule.IsWide)
            {
                Engine.Instance.GraphicsDevice.ScissorRectangle = new Rectangle(
                    55,
                    0,
                    420 - 110,
                    240
                );
            }
        });

        return cursor.Generate();
    }

    internal sealed class ScrollLayerHooks
    {
        public static void Load(IHarmony harmony)
        {
            harmony.Patch(
                AccessTools.DeclaredConstructor(typeof(Background.ScrollLayer), [typeof(Level), typeof(XmlElement)]),
                postfix: new HarmonyMethod(ScrollLayer_ctor_Postfix)
            );
        }

        private static void ScrollLayer_ctor_Postfix(Background.ScrollLayer __instance)
        {
            if (WiderSetModule.IsWide)
            {
                __instance.WrapSize.X = Math.Max(420f, __instance.Image.Width);
            }
        }
    }

    internal sealed class LightningFlashLayerHooks
    {
        public static void Load(IHarmony harmony)
        {
            harmony.Patch(
                AccessTools.DeclaredMethod(typeof(Background.LightningFlashLayer), nameof(Background.LightningFlashLayer.Render)),
                transpiler: new HarmonyMethod(LightningFlashLayer_Render_Transpiler)
            );
        }

        private static IEnumerable<CodeInstruction> LightningFlashLayer_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var cursor = new ILTranspilerCursor(generator, instructions);

            cursor.GotoNext(
                MoveType.After,
                [
                    ILMatch.LdcR4(320f)
                ]
            );

            cursor.EmitDelegate((float x) =>
            {
                if (WiderSetModule.IsWide)
                {
                    return x + 100;
                }

                return x;
            });

            return cursor.Generate();
        }
    }

    internal sealed class VortexLayerHooks
    {
        public static void Load(IHarmony harmony)
        {
            harmony.Patch(
                AccessTools.DeclaredMethod(typeof(Background.VortexLayer), "GetRandomBezier"),
                transpiler: new HarmonyMethod(VortexLayer_GetRandomBezier_Transpiler)
            );

            harmony.Patch(
                AccessTools.DeclaredMethod(typeof(Background.VortexLayer), nameof(Background.VortexLayer.Render)),
                transpiler: new HarmonyMethod(VortexLayer_Render_Transpiler)
            );
        }

        private static IEnumerable<CodeInstruction> VortexLayer_GetRandomBezier_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var cursor = new ILTranspilerCursor(generator, instructions);

            cursor.GotoNext(
                MoveType.After,
                [
                    ILMatch.LdcR4(100f)
                ]
            );

            cursor.EmitDelegate((float x) =>
            {
                if (WiderSetModule.IsWide)
                {
                    return x + 50;
                }

                return x;
            });

            cursor.GotoNext(
                MoveType.After,
                [
                    ILMatch.LdcR4(155f)
                ]
            );

            cursor.EmitDelegate((float x) =>
            {
                if (WiderSetModule.IsWide)
                {
                    return x + 50;
                }

                return x;
            });

            return cursor.Generate();
        }

        private static IEnumerable<CodeInstruction> VortexLayer_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var cursor = new ILTranspilerCursor(generator, instructions);

            cursor.GotoNext(
                MoveType.After,
                [
                    ILMatch.LdcR4(320f)
                ]
            );

            cursor.EmitDelegate((float x) =>
            {
                if (WiderSetModule.IsWide)
                {
                    return x + 100;
                }

                return x;
            });

            return cursor.Generate();
        }
    }
}

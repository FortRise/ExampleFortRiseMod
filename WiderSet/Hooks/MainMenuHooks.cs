using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using TowerFall;

namespace Teuria.WiderSet;

internal static class UnsafeSpriteBatch
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "transformMatrix")]
    public static extern ref Matrix GetSpriteBatchTransformMatrix(SpriteBatch batch);
}

internal sealed class MainMenuHooks : IHookable
{
    private static Tween? tween;

    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MainMenu), nameof(MainMenu.Begin)),
            postfix: new HarmonyMethod(MainMenu_Begin_Postfix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MainMenu), nameof(MainMenu.Update)),
            new HarmonyMethod(MainMenu_Update_Prefix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MainMenu), nameof(MainMenu.orig_Render)),
            transpiler: new HarmonyMethod(MainMenu_Render_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MainMenu), "CreateMain"),
            new HarmonyMethod(MainMenu_CreateMain_Prefix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MainMenu), nameof(MainMenu.CreateRollcall)),
            transpiler: new HarmonyMethod(PlayerAmountUtilities.EightPlayerTranspiler)
        );
    }

    private static IEnumerable<CodeInstruction> MainMenu_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);
        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(0f)]))
            {
                cursor.EmitDelegate((float f) => 1f);
            }
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> MainMenu_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Callvirt("Begin")
            ]
        );

        cursor.EmitDelegate(() =>
        {
            ref Matrix matrix = ref UnsafeSpriteBatch.GetSpriteBatchTransformMatrix(Draw.SpriteBatch);
            matrix = Engine.Instance.Screen.Matrix;
        });

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Brtrue_S(),
                ILMatch.LdcI4(0)
            ]
        );

        cursor.EmitDelegate((int width) => width - 210);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcI4(320)
            ]
        );

        cursor.EmitDelegate((int width) => width + 100);

        return cursor.Generate();
    }

    private static void MainMenu_Begin_Postfix(MainMenu __instance)
    {
        var entity = new Entity();
        tween = Tween.Create(Tween.TweenMode.Persist, Ease.CubeOut, 60);
        entity.Add(tween);
        __instance.Add(entity);
        __instance.Camera.X -= Screen.LeftImage.Width - 2;
        __instance.UILayer.Camera.X -= Screen.LeftImage.Width - 2;
    }

    private static void MainMenu_CreateMain_Prefix()
    {
        WiderSetModule.IsWide = false;
        WrapMath.AddWidth = new Vector2(320, 0);
    }

    private static void MainMenu_Update_Prefix()
    {
        if (tween is null)
        {
            return;
        }

        if (WiderSetModule.DirtyWide)
        {
            tween.Stop();
            if (!WiderSetModule.IsWide && !WiderSetModule.AboutToGetWide)
            {
                tween.OnUpdate = t =>
                {
                    Engine.Instance.Screen.PadOffset = MathHelper.Lerp(Engine.Instance.Screen.PadOffset, 0f, t.Eased);
                };
            }
            else
            {
                tween.OnUpdate = t =>
                {
                    Engine.Instance.Screen.PadOffset = MathHelper.Lerp(Engine.Instance.Screen.PadOffset, -60f, t.Eased);
                };
            }

            tween.Start();
        }
    }
}

using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class MenuBackgroundHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MenuBackground), nameof(MenuBackground.Render)),
            prefix: new HarmonyMethod(MenuBackground_Render_Prefix),
            transpiler: new HarmonyMethod(MenuBackground_Render_Transpiler)
        );
    }

    private static void MenuBackground_Render_Prefix()
    {
        Draw.Rect(new Rectangle(-Screen.LeftImage.Width, (int)Engine.Instance.Scene.Camera.Y, 420, 240), new Color(35, 23, 89));
    }

    private static IEnumerable<CodeInstruction> MenuBackground_Render_Transpiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator
    )
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcR4(-1)
            ]
        );

        cursor.EmitDelegate((float left) => left + Engine.Instance.Screen.PadOffset);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcI4(320)
            ]
        );

        cursor.EmitDelegate((int right) => (int)(right - Engine.Instance.Screen.PadOffset));

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> MenuBackgroundScreenWidthTranspiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcR4(320)
            ]
        );

        cursor.EmitDelegate((float width) => width + 100);

        return cursor.Generate();
    }
}
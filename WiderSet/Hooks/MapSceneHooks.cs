using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class MapSceneHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MapScene), nameof(MapScene.Render)),
            transpiler: new HarmonyMethod(MapScene_Render_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MapScene), nameof(MapScene.ClampCamera)),
            transpiler: new HarmonyMethod(MapScene_ClampCamera_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MapScene), nameof(MapScene.FixedClampCamera)),
            transpiler: new HarmonyMethod(MapScene_ClampCamera_Transpiler)
        );
       
        MethodBase methodBase = null!;
        var methods = typeof(MapScene).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var method in methods)
        {
            if (method.Name.Contains("<InitVersusButtons>"))
            {
                methodBase = method;
            }
        }
        
        harmony.Patch(
            methodBase,
            postfix: new HarmonyMethod(InitVersusButtons_Postfix)
        );
    }

    private static void InitVersusButtons_Postfix(MapScene __instance, VersusTowerData x, ref bool __result)
    {
        if (WiderSetModule.IsWide)
        {
            __result = __result && x.TowerSet.Contains("<Teuria.WiderSet>");
        }
    }

    private static IEnumerable<CodeInstruction> MapScene_ClampCamera_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcR4(160f)
            ]
        );

        cursor.EmitDelegate((float width) =>
        {
            if (WiderSetModule.IsWide)
            {
                return width + 50f;
            }

            return width;
        });

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcI4(160)
            ]
        );

        cursor.EmitDelegate((int width) =>
        {
            if (WiderSetModule.IsWide)
            {
                return width + 50;
            }

            return width;
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> MapScene_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
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
                ILMatch.LdcR4(320)
            ]
        );

        cursor.EmitDelegate((float width) => width + 100f);

        return cursor.Generate();
    }
}

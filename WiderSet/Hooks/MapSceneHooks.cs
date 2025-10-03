using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;
using TowerFall.Patching;

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

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MapScene), nameof(MapScene.InitVersusButtons)),
            transpiler: new HarmonyMethod(MapScene_InitVersusButtons_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> MapScene_InitVersusButtons_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x => 
        {
            while (x.Next(MoveType.After, [ILMatch.Ldfld("Levels")]))
            {
                cursor.Emit(new CodeInstruction(OpCodes.Ldloc, 4));
                cursor.EmitDelegate((List<VersusLevelData> levels, VersusTowerData data) => 
                {
                    if (WiderSetModule.IsWide)
                    {
                        string levelID = data.GetLevelID();
                        return WideTowerManager.Instance.MappedLevels[levelID];
                    }

                    return levels;
                });
            }
        });

        return cursor.Generate();
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

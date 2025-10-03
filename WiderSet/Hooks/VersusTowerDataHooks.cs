using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class VersusTowerDataHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(VersusTowerData), nameof(VersusTowerData.GetLevels)),
            transpiler: new HarmonyMethod(VersusTowerData_GetLevels_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> VersusTowerData_GetLevels_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x => 
        {
            while (x.Next(MoveType.After, [ILMatch.Ldfld("Levels")]))
            {
                cursor.Emit(new CodeInstruction(OpCodes.Ldarg_0));
                cursor.EmitDelegate((List<VersusLevelData> levels, VersusTowerData __instance) => 
                {
                    if (WiderSetModule.IsWide)
                    {
                        string levelID = __instance.GetLevelID();
                        if (__instance.GetLevelSet() == "Teuria.WiderSet/Teuria.WiderSet")
                        {
                            return levels;
                        }

                        return WideTowerManager.Instance.MappedLevels[levelID];
                    }

                    return levels;
                });
            }
        });

        return cursor.Generate();
    }
}


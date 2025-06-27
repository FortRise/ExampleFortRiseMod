using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class FadingArrow : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(
                typeof(Arrow), nameof(Arrow.Update)
            ),
            postfix: new HarmonyMethod(Arrow_Update_Postfix),
            transpiler: new HarmonyMethod(Arrow_Update_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(
                typeof(Arrow), nameof(Arrow.Create)
            ),
            postfix: new HarmonyMethod(Arrow_Create_Postfix)
        );
    }

    private static void Arrow_Create_Postfix(in Arrow __result)
    {
        __result.StopFlashing();
    }

    private static IEnumerable<CodeInstruction> Arrow_Update_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Call("get_Flashing")
            ]
        );

        cursor.Emit(new CodeInstruction(OpCodes.Ldarg_0));
        cursor.EmitDelegate((bool flashing, Arrow self) =>
        {
            if (Variants.FadingArrow.IsActive(self.PlayerIndex))
            {
                return false;
            }
            return flashing;
        });

        return cursor.Generate();
    }

    private static void Arrow_Update_Postfix(Arrow __instance)
    {
        if (__instance is LaserArrow || !Variants.FadingArrow.IsActive(__instance.PlayerIndex))
        {
            return;
        }

        if (!__instance.Flashing && __instance.State >= TowerFall.Arrow.ArrowStates.Stuck)
        {
            __instance.Flash(60, () =>
            {
                __instance.StopFlashing();
                __instance.RemoveSelf();
            });
        }
    }
}
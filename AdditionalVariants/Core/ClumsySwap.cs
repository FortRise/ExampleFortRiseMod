using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class ClumsySwap : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), nameof(Player.Update)),
            transpiler: new HarmonyMethod(Player_Update_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> Player_Update_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext([
            ILMatch.Ldsfld("sfx_arrowToggle")
        ]);

        cursor.Emit(new CodeInstruction(OpCodes.Ldarg_0));
        cursor.EmitDelegate((Player player) =>
        {
            if (Variants.ClumsySwap.IsActive(player.PlayerIndex))
            {
                DynamicData.For(player).Invoke("DropArrow");
            }
        });

        return cursor.Generate();
    }
}
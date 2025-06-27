using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class NoArrowTinks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Arrow), nameof(Arrow.Update)),
            transpiler: new HarmonyMethod(Arrow_Update_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> Arrow_Update_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        int ldlocIndex = AdditionalVariantsModule.Instance.Context.Flags.IsWindows ? 3 : 2;

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Ldloc(ldlocIndex),
                ILMatch.CallOrCallvirt("get_Dangerous")
            ]
        );

        cursor.EmitDelegate((bool isDangerous) => isDangerous && !Variants.NoArrowTinks.IsActive());

        return cursor.Generate();
    }
}
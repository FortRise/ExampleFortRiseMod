using FortRise;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

public sealed class TFGameHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(TFGame), "orig_Initialize"),
            transpiler: new HarmonyMethod(PlayerAmountUtilities.EightPlayerTranspilerNoCondition)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(TFGame), nameof(TFGame.CharacterTaken)),
            transpiler: new HarmonyMethod(PlayerAmountUtilities.EightPlayerTranspilerNoCondition)
        );
    }
}

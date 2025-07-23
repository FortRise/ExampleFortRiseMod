using FortRise;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class MatchSettingsHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MatchSettings), nameof(MatchSettings.GetMaxTeamSize)),
            transpiler: new HarmonyMethod(PlayerAmountUtilities.EightPlayerTranspiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MatchSettings), nameof(MatchSettings.GetPlayerTeamSize)),
            transpiler: new HarmonyMethod(PlayerAmountUtilities.EightPlayerTranspiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MatchSettings), nameof(MatchSettings.GetTeamMismatch)),
            transpiler: new HarmonyMethod(PlayerAmountUtilities.EightPlayerTranspiler)
        );

        harmony.Patch(
            AccessTools.DeclaredPropertyGetter(typeof(MatchSettings), nameof(MatchSettings.CanStartWithTeams)),
            transpiler: new HarmonyMethod(PlayerAmountUtilities.EightPlayerTranspiler)
        );
    }
}
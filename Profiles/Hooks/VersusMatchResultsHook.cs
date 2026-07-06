using HarmonyLib;
using TowerFall;

namespace Teuria.Profiles;

[HarmonyPatch(typeof(VersusMatchResults))]
internal static class VersusMatchResultsHook
{
    [HarmonyPatch(MethodType.Constructor, [typeof(Session), typeof(VersusRoundResults)])]
    [HarmonyPostfix]
    public static void VersusMatchResultsConstructor_Prefix(Session session)
    {
        var wideSetPlayer = ProfilesModule.Instance.IWiderSetModAPI is { IsWide: true } ? 8 : 4;
        var winner = session.GetWinner();
        for (int i = 0; i < wideSetPlayer; i++)
        {
            if (TFGame.Players[i] && winner == session.GetScoreIndex(i))
            {
                ProfileSessionStats.RegisterArcherWin(i);
            }
        }
        ProfileSessionStats.RegisterArcherPlays();
    }
}

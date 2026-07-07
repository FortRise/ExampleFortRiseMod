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
                var win = ProfilesModule.Instance.ProfileActive[i];
                if (win is not null)
                {
                    var saveData = ProfilesModule.Instance.GetSaveData<ProfileSaveData>()!;

                    foreach (var data in saveData.ProfileStats)
                    {
                        if (data.Name == win.Name)
                        {
                            data.Wins += 1;
                            goto BREAKOUT;
                        }
                    }

                    saveData.ProfileStats.Add(new ProfileStats() { Name = win.Name, Wins = 1 });
                }

                BREAKOUT:

                ProfileSessionStats.RegisterArcherWin(i);
            }
        }
        ProfileSessionStats.RegisterArcherPlays();
    }
}

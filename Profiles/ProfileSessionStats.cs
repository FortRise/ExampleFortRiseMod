using System.Collections.Generic;
using TowerFall;

namespace Teuria.Profiles;

public static class ProfileSessionStats
{
    public static List<int> ArcherPlays = null!;
    public static List<int> ArcherKills = null!;
    public static List<int> ArcherDeaths = null!;
    public static List<int> ArcherSelfKills = null!;
    public static List<int> ArcherWins = null!;

    public static void Initialize()
    {
        ArcherPlays = new (ProfilesModule.Instance.Profiles.Count);
        ArcherKills = new (ProfilesModule.Instance.Profiles.Count);
        ArcherDeaths = new (ProfilesModule.Instance.Profiles.Count);
        ArcherSelfKills = new (ProfilesModule.Instance.Profiles.Count);
        ArcherWins = new (ProfilesModule.Instance.Profiles.Count);

        for (int i = 0; i < ProfilesModule.Instance.Profiles.Count; i += 1)
        {
            ArcherPlays.Add(0);
            ArcherKills.Add(0);
            ArcherDeaths.Add(0);
            ArcherSelfKills.Add(0);
            ArcherWins.Add(0);
        }
    }

    public static void AddOne()
    {
        ArcherPlays.Add(0);
        ArcherKills.Add(0);
        ArcherDeaths.Add(0);
        ArcherSelfKills.Add(0);
        ArcherWins.Add(0);
    }

    public static void RemoveOne(int index)
    {
        ArcherPlays.RemoveAt(index);
        ArcherKills.RemoveAt(index);
        ArcherDeaths.RemoveAt(index);
        ArcherSelfKills.RemoveAt(index);
        ArcherWins.RemoveAt(index);
    }

    public static void RegisterArcherPlays()
    {
        for (int i = 0; i < ProfilesModule.Instance.ProfileActive.Length; i++)
        {
            var profile = ProfilesModule.Instance.ProfileActive[i];
            if (TFGame.Players[i] && profile is not null)
            {
                var idx = ProfilesModule.Instance.Profiles.IndexOf(profile);
                ArcherPlays[idx] += 1;
            }
        }
    }

    public static void RegisterVersusKill(int killerIndex, int killedIndex, bool teamKill)
    {
        int killerIndexP = -1;
        if (killerIndex != -1)
        {
            var killerProfile = ProfilesModule.Instance.ProfileActive[killerIndex];
            killerIndexP = ProfilesModule.Instance.Profiles.IndexOf(killerProfile!);
        }

        var killedProfile = ProfilesModule.Instance.ProfileActive[killedIndex];

        int killedIndexP = ProfilesModule.Instance.Profiles.IndexOf(killedProfile!);

        if (killedIndexP != -1)
        {
            ArcherDeaths[killedIndexP] += 1;
        }

        if (teamKill || killerIndex == killedIndex || killerIndex == -1)
        {
            if (killedIndexP != -1)
            {
                ArcherSelfKills[killedIndex] += 1;
            }
        }
        else
        {
            if (killerIndexP != -1)
            {
                ArcherKills[killerIndexP] += 1;
            }
        }
    }

    public static void RegisterArcherWin(int playerIndex)
    {
        var winner = ProfilesModule.Instance.ProfileActive[playerIndex];
        int winnerIndex = ProfilesModule.Instance.Profiles.IndexOf(winner!);
        if (winnerIndex != -1)
        {
            ArcherWins[winnerIndex]++;
        }
    }

    public static string GetWinRatio(int profileIndex)
    {
        float win;
        if (ArcherPlays[profileIndex] == 0 || ArcherWins[profileIndex] == 0)
        {
            win = 0f;
        }
        else
        {
            win = ArcherWins[profileIndex] / (float)ArcherPlays[profileIndex];
        }
        return win.ToString("F");
    }

    public static string GetKillDeathRatio(int profileIndex)
    {
        float kd;
        if (ArcherDeaths[profileIndex] == 0)
        {
            kd = ArcherKills[profileIndex];
        }
        else if (ArcherKills[profileIndex] == 0)
        {
            kd = 0f;
        }
        else
        {
            kd = ArcherKills[profileIndex] / (float)ArcherDeaths[profileIndex];
        }
        return kd.ToString("F");
    }
}

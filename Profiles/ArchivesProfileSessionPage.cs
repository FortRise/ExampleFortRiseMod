using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.Profiles;

public class ArchivesProfileSessionPage : ArchivesPage
{
    private record struct ProfileSessionStat(
        string Name, 
        int ArcherPlays, 
        string Wins,
        string WinRatio,
        string Kills,
        string Deaths,
        string SelfKills,
        string KillDeathRatio,
        Color Color
    );

    private List<ProfileSessionStat> sessionStats = [];
    private float moveY;
    private float maxMoveY;

    public ArchivesProfileSessionPage() : base("PROFILE STATS")
    {
        string[] stats = ["WINS", "RATE", "KILLS", "DEATHS", "SELF", "K/D"];
        for (int i = 0; i < stats.Length; i++)
        {
            Add(new OutlineText(TFGame.Font, stats[i], new Vector2(-55f + 35f * i, 5f), Text.HorizontalAlign.Center, Text.VerticalAlign.Center));
        }

        float offsetY = 20f;
        var activeProfiles = ProfilesModule.Instance.EnabledProfile;

        List<ProfileSessionStat> hasNotPlayed = [];

        for (int j = 0; j < activeProfiles.Count; j += 1)
        {
            var profile = activeProfiles[j];

            var i = ProfilesModule.Instance.Profiles.IndexOf(profile);

            Color colorB;
            if (profile.SelectedArchers.Count > 0)
            {
                ArcherData archerData = profile.SelectedArchers[0].ArcherType switch
                {
                    ArcherData.ArcherTypes.Normal => ArcherData.Archers[profile.FirstArcherIndex],
                    ArcherData.ArcherTypes.Alt => ArcherData.AltArchers[profile.FirstArcherIndex],
                    ArcherData.ArcherTypes.Secret => ArcherData.SecretArchers[profile.FirstArcherIndex],
                    _ => throw new UnreachableException()
                };

                colorB = archerData.ColorB;
            }
            else
            {
                colorB = Color.White;
            }

            var profileSessionStat = new ProfileSessionStat(
                profile.Name.ToUpperInvariant(), 
                ProfileSessionStats.ArcherPlays[i],
                ProfileSessionStats.ArcherWins[i].ToString(),
                ProfileSessionStats.GetWinRatio(i),
                ProfileSessionStats.ArcherKills[i].ToString(),
                ProfileSessionStats.ArcherDeaths[i].ToString(),
                ProfileSessionStats.ArcherSelfKills[i].ToString(),
                ProfileSessionStats.GetKillDeathRatio(i), colorB
            );

            if (ProfileSessionStats.ArcherPlays[i] == 0)
            {
                hasNotPlayed.Add(profileSessionStat);
            }
            else
            {
                sessionStats.Add(profileSessionStat);
            }
        }

        sessionStats.AddRange(hasNotPlayed);

        maxMoveY = (activeProfiles.Count * 10) - 50;

        offsetY += 60f;


        Add(new OutlineText(TFGame.Font, "KILLS BY PROFILE", new Vector2(-90, offsetY)));

        offsetY += 10f;

        var profileSaveData = ProfilesModule.Instance.GetSaveData<ProfileSaveData>()!;
        var count = activeProfiles.Count;

        List<BarChart.ChartData> chartDatas = new List<BarChart.ChartData>();
        
        for (int j = 0; j < count; j += 1)
        {
            var profile = activeProfiles[j];
            var p = profileSaveData.ProfileStats.FirstOrDefault(x => x.Name == profile.Name);
            if (p is null)
            {
                continue;
            }

            if (p.Kills == 0)
            {
                continue;
            }

            Color colorB;
            if (profile.SelectedArchers.Count > 0)
            {
                ArcherData archerData = profile.SelectedArchers[0].ArcherType switch
                {
                    ArcherData.ArcherTypes.Normal => ArcherData.Archers[profile.FirstArcherIndex],
                    ArcherData.ArcherTypes.Alt => ArcherData.AltArchers[profile.FirstArcherIndex],
                    ArcherData.ArcherTypes.Secret => ArcherData.SecretArchers[profile.FirstArcherIndex],
                    _ => throw new UnreachableException()
                };

                colorB = archerData.ColorB;
            }
            else
            {
                colorB = Color.White;
            }

            chartDatas.Add(new BarChart.ChartData(colorB, p.Name.ToUpperInvariant(), p.Kills));
        }

        BarChart chart;
        Add(chart = new BarChart(new Vector2(-125, offsetY), 60, [.. chartDatas]));

        offsetY -= 10;

        Add(new OutlineText(TFGame.Font, "WINS BY PROFILE", new Vector2(40, offsetY)));

        offsetY += 10f;

        chartDatas = new List<BarChart.ChartData>();
        
        for (int j = 0; j < count; j += 1)
        {
            var profile = activeProfiles[j];
            var p = profileSaveData.ProfileStats.FirstOrDefault(x => x.Name == profile.Name);
            if (p is null)
            {
                continue;
            }

            if (p.Wins == 0)
            {
                continue;
            }

            Color colorB;
            if (profile.SelectedArchers.Count > 0)
            {
                ArcherData archerData = profile.SelectedArchers[0].ArcherType switch
                {
                    ArcherData.ArcherTypes.Normal => ArcherData.Archers[profile.FirstArcherIndex],
                    ArcherData.ArcherTypes.Alt => ArcherData.AltArchers[profile.FirstArcherIndex],
                    ArcherData.ArcherTypes.Secret => ArcherData.SecretArchers[profile.FirstArcherIndex],
                    _ => throw new UnreachableException()
                };

                colorB = archerData.ColorB;
            }
            else
            {
                colorB = Color.White;
            }

            chartDatas.Add(new BarChart.ChartData(colorB, p.Name.ToUpperInvariant(), p.Wins));
        }

        Add(chart = new BarChart(new Vector2(0, offsetY), 60, [.. chartDatas]));
        offsetY += 40;
    }

    public override void Update()
    {
        base.Update();

        if (!IsFocused)
        {
            return;
        }

        if (MenuInput.DownCheck)
        {
            moveY = Math.Max(-maxMoveY, moveY - 2 * Engine.TimeMult);
        }

        else if (MenuInput.UpCheck)
        {
            moveY = Math.Min(0, moveY + 2 * Engine.TimeMult);
        }
    }

    public override void Render()
    {
        base.Render();

        var pos = Position;

        for (int i = 0; i < sessionStats.Count; i += 1)
        {
            int x = 125;
            float offsetY = moveY + i * 10;

            float alpha = 1;

            if (offsetY < 0)
            {
                alpha = MathHelper.Clamp((offsetY + 10) / 10, 0, 1);
            }
            else if (offsetY > 40)
            {
                alpha = MathHelper.Clamp((50 - offsetY) / 10, 0, 1);
            }

            var stats = sessionStats[i];
            Draw.OutlineTextCentered(TFGame.Font, stats.Name, new Vector2(pos.X - x + 25, pos.Y + 20 + offsetY), stats.Color * alpha, Color.Black * alpha);

            x -= 70;

            if (stats.ArcherPlays > 0)
            {
                Draw.OutlineTextCentered(TFGame.Font, stats.Wins, new Vector2(pos.X - x, pos.Y + 20 + offsetY), stats.Color * alpha, Color.Black * alpha);
                x -= 35;

                Draw.OutlineTextCentered(TFGame.Font, stats.WinRatio, new Vector2(pos.X - x, pos.Y + 20 + offsetY), stats.Color * alpha, Color.Black * alpha);
                x -= 35;

                Draw.OutlineTextCentered(TFGame.Font, stats.Kills, new Vector2(pos.X - x, pos.Y + 20 + offsetY), stats.Color * alpha, Color.Black * alpha);
                x -= 35;

                Draw.OutlineTextCentered(TFGame.Font, stats.Deaths, new Vector2(pos.X - x, pos.Y + 20 + offsetY), stats.Color * alpha, Color.Black * alpha);
                x -= 35;

                Draw.OutlineTextCentered(TFGame.Font, stats.SelfKills, new Vector2(pos.X - x, pos.Y + 20 + offsetY), stats.Color * alpha, Color.Black * alpha);
                x -= 35;

                Draw.OutlineTextCentered(TFGame.Font, stats.KillDeathRatio, new Vector2(pos.X - x, pos.Y + 20 + offsetY), stats.Color * alpha, Color.Black * alpha);
            }
            else
            {
                int offsetX2 = 0;
                while (offsetX2 < 6f)
                {
                    Draw.OutlineTextCentered(TFGame.Font, "-", new Vector2(pos.X + -55f + 35f * offsetX2, pos.Y + 20 + offsetY), stats.Color * alpha, Color.Black * alpha);
                    offsetX2++;
                }
            }
        }
    }
}

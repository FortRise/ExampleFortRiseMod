using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.Profiles;

public class ArchivesProfileSessionPage : ArchivesPage
{
    private float scrollY;
    private float maxScrollY;

    public ArchivesProfileSessionPage() : base("PROFILE STATS")
    {
        string[] stats = ["WINS", "RATE", "KILLS", "DEATHS", "SELF", "K/D"];
        for (int i = 0; i < stats.Length; i++)
        {
            Add(new OutlineText(TFGame.Font, stats[i], new Vector2(-55f + 35f * i, 5f), Text.HorizontalAlign.Center, Text.VerticalAlign.Center));
        }

        float offsetY = 20f;
        for (int i = 0; i < ProfilesModule.Instance.Profiles.Count; i += 1)
        {
            var profile = ProfilesModule.Instance.Profiles[i];
            ArcherData archerData = profile.ArcherTypes switch
            {
                ArcherData.ArcherTypes.Normal => ArcherData.Archers[profile.ArcherIndex],
                ArcherData.ArcherTypes.Alt => ArcherData.AltArchers[profile.ArcherIndex],
                ArcherData.ArcherTypes.Secret => ArcherData.SecretArchers[profile.ArcherIndex],
                _ => throw new UnreachableException()
            };

            Color colorB = archerData.ColorB;
            float offsetX = -125f;

            Add(new OutlineText(TFGame.Font, profile.Name.ToUpperInvariant(), new Vector2(offsetX + 25, offsetY))
            {
                Color = colorB
            });

            offsetX += 70;

            if (ProfileSessionStats.ArcherPlays[i] > 0)
            {
                Add(new OutlineText(TFGame.Font, ProfileSessionStats.ArcherWins[i].ToString(), new Vector2(offsetX, offsetY))
                {
                    Color = colorB
                });

                offsetX += 35f;

                Add(new OutlineText(TFGame.Font, ProfileSessionStats.GetWinRatio(i), new Vector2(offsetX, offsetY))
                {
                    Color = colorB
                });

                offsetX += 35f;
                Add(new OutlineText(TFGame.Font, ProfileSessionStats.ArcherKills[i].ToString(), new Vector2(offsetX, offsetY))
                {
                    Color = colorB
                });

                offsetX += 35f;
                Add(new OutlineText(TFGame.Font, ProfileSessionStats.ArcherDeaths[i].ToString(), new Vector2(offsetX, offsetY))
                {
                    Color = colorB
                });

                offsetX += 35f;
                Add(new OutlineText(TFGame.Font, ProfileSessionStats.ArcherSelfKills[i].ToString(), new Vector2(offsetX, offsetY))
                {
                    Color = colorB
                });

                offsetX += 35f;
                Add(new OutlineText(TFGame.Font, ProfileSessionStats.GetKillDeathRatio(i), new Vector2(offsetX, offsetY))
                {
                    Color = colorB
                });
            }
            else
            {
                int offsetX2 = 0;
                while (offsetX2 < 6f)
                {
                    Add(new OutlineText(TFGame.Font, "-", new Vector2(-55f + 35f * offsetX2, offsetY))
                    {
                        Color = colorB
                    });
                    offsetX2++;
                }
            }


            offsetY += 10f;
        }

        maxScrollY = offsetY % 240f * ((int)offsetY / 240);
    }

    public override void Added()
    {
        base.Added();
        MainMenu.MaxUICameraY = maxScrollY;
    }

    public override void Update()
    {
        base.Update();
        if (!IsFocused)
        {
            return;
        }

        if (MenuInput.LeftCheck || MenuInput.RightCheck)
        {
            MainMenu.TweenUICameraToY(0, 0);
            scrollY = 0;
            return;
        }

        if (MenuInput.DownCheck)
        {
            scrollY = Math.Min(maxScrollY, scrollY + 4f * Engine.TimeMult);
        }
        else if (MenuInput.UpCheck)
        {
            scrollY = Math.Max(0, scrollY - 4f * Engine.TimeMult);
        }

        if (MainMenu is not null)
        {
            MainMenu.TweenUICameraToY(Math.Max(0f, scrollY), 10);
        }
    }
}

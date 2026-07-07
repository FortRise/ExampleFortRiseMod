using System.Collections.Generic;
using System.Linq;
using FortRise;
using Microsoft.Xna.Framework;
using TowerFall;

namespace Teuria.Profiles;

public sealed class SelectArcherState : CustomMenuState
{
    public SelectArcherState(MainMenu main) : base(main)
    {
    }

    public override void Create()
    {
        var archerBundle = BundleStateManager.Instance.Pop();
        var profile = archerBundle.Get<PlayerProfile>("profile");

        const int GridAmount = 4;
        List<ArcherPortraitOptionsButton> portraits = [];
        int sum = 0;

        var archers = ArcherData.Archers.Concat(ArcherData.AltArchers)
            .Where(x => x is not null)
            .ToArray();

        for (int i = 0; i < archers.Length; i += 1)
        {
            var archer = archers[i];
            int gridX = i % GridAmount;
            int gridY = i / GridAmount;

            int posX = gridX * 60;
            int posY = gridY * 60;

            var tweenFrom = (gridY & 1) == 0 ? new Vector2(posX + 400, posY + 50) : new Vector2(posX - 400, posY + 50);

            var portrait = new ArcherPortraitOptionsButton(new Vector2(posX + (160 - 110), posY + 50), tweenFrom, archer, (x, y) =>
            {
                profile.ArcherID = x;
                profile.ArcherTypes = y;

                Main.State = ProfilesModule.Instance.ManageProfileState.MenuState;
            });
            portraits.Add(portrait);

            sum += posY;
        }

        for (int x = 0; x < portraits.Count; x += 1)
        {
            var portrait = portraits[x];

            if (x != 0)
            {
                portrait.LeftItem = portraits[x - 1];
            }

            if (x + 1 < portraits.Count)
            {
                portrait.RightItem = portraits[x + 1];
            }

            if (x - GridAmount >= 0)
            {
                portrait.UpItem = portraits[x - GridAmount];
            }

            if (x + GridAmount < portraits.Count)
            {
                portrait.DownItem = portraits[x + GridAmount];
            }
        }

        Main.MaxUICameraY = sum - 40;
        Main.Add(portraits);

        Main.ToStartSelected = portraits[0];
        Main.BackState = ProfilesModule.Instance.ManageProfileState.MenuState;
    }

    public override void Destroy()
    {
    }
}

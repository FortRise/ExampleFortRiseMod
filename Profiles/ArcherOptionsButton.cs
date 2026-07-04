using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.Profiles;

public class ArcherOptionsButton : OptionsButton
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "changedWiggler")]
    private static extern ref Wiggler changedWiggler(OptionsButton target);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "wiggleDir")]
    private static extern ref int wiggleDir(OptionsButton target);

    private ArcherData? archerData;
    private Sprite<string>? gem;

    public ArcherOptionsButton(string title, string? archerDataName) : base(title)
    {
        if (archerDataName is null)
        {
            return;
        }
        var entry = ProfilesModule.Instance.Context.Registry.Archers.RegisteredArchers
            .Select(x => x.Value)
            .FirstOrDefault(x => x.Name == archerDataName);

        if (entry is null)
        {
            archerData = ArcherData.Archers.Union(ArcherData.AltArchers)
                .Union(ArcherData.SecretArchers)
                .Where(x => x is not null)
                .FirstOrDefault(x => x.Name0 + x.Name1 == archerDataName);
        }
        else
        {
            archerData = entry.ArcherData;
        }


        if (archerData is not null)
        {
            gem = TFGame.MenuSpriteData.GetSpriteString(archerData.Gems.Menu);
        }
    }

    public override void Render()
    {
        Vector2 middle = new Vector2(30f + 2f * changedWiggler(this).Value * wiggleDir(this), 0f);

        var pos = new Vector2(Position.X, Position.Y) + middle;
    
        base.Render();

        if (gem is not null)
        {
            gem.Position = pos;
            gem.Render();
        }
    }
}
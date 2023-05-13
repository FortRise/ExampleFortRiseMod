using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace ExampleMod;

[CustomArrows("SillyBramble")]
public class SillyBramble : BrambleArrow 
{
    // This is automatically been set by the mod loader
    public override ArrowTypes ArrowType { get; set; }
    private DynData<BrambleArrow> privateProp;
    private Image buriedImage;
    private Image normalImage;

    public SillyBramble() : base()
    {
        privateProp = new DynData<BrambleArrow>(this);
        this.buriedImage = privateProp.Get<Image>("buriedImage");
        this.normalImage = privateProp.Get<Image>("normalImage");
    }

    public static ArrowInfo CreateGraphicPickup() 
    {
        var graphic = new Sprite<int>(TFGame.Atlas["pickups/bombArrows"], 12, 12, 0);
        graphic.Add(0, 0.3f, new int[2] { 0, 1 });
        graphic.Play(0, false);
        graphic.CenterOrigin();
        return ArrowInfo.Create(graphic);
    }

    protected override void HitWall(TowerFall.Platform platform)
    {
        base.HitWall(platform);
        Explode();
    }

    private void Explode()
    {
        bool plusOneKill = base.State == Arrow.ArrowStates.Buried && base.BuriedIn.Entity is PlayerCorpse && !base.Level.Session.MatchSettings.IsFriendly((base.BuriedIn.Entity as PlayerCorpse).PlayerIndex, base.PlayerIndex);
        Vector2 at = (this.Position + Calc.AngleToVector(base.Direction + 3.1415927f, 10f)).Floor();
        if (!Explosion.Spawn(base.Level, at, base.PlayerIndex, plusOneKill, false, false))
        {
            Explosion.Spawn(base.Level, this.Position, base.PlayerIndex, plusOneKill, false, false);
        }
        Sounds.pu_bombArrowExplode.Play(base.X, 1f);
    }
}
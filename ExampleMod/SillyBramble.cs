using System.Reflection;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace ExampleMod;

[CustomArrows("TriggerBrambleArrows", "CreateGraphicPickup")]
public class TriggerBrambleArrow : TriggerArrow
{
    // This is automatically been set by the mod loader
    public override ArrowTypes ArrowType { get; set; }
    private bool used, canDie;
    private static Action<TriggerArrow, LevelEntity, Vector2, float> BaseInit;


    public static ArrowInfo CreateGraphicPickup() 
    {
        var graphic = new Sprite<int>(TFGame.Atlas["pickups/bombArrows"], 12, 12, 0);
        graphic.Add(0, 0.3f, new int[2] { 0, 1 });
        graphic.Play(0, false);
        graphic.CenterOrigin();
        var arrowInfo = ArrowInfo.Create(graphic, TFGame.Atlas["player/arrowHUD/brambleArrow"]);
        arrowInfo.Name = "Trigger Bramble Arrows";
        return arrowInfo;
    }

    public TriggerBrambleArrow() : base()
    {
    }

    protected override bool CheckForTargetCollisions()
    {
        foreach (Entity entity in base.Level[GameTags.Target])
        {
            var levelEntity = (LevelEntity)entity;
            if (levelEntity.ArrowCheck(this) && levelEntity != this.CannotHit)
            {
                Vector2 vector = (levelEntity.Position - (this.Position - this.Speed)).SafeNormalize();
                levelEntity.OnSqueakyBounce(this, vector);
                base.State = Arrow.ArrowStates.Falling;
                this.Speed = vector * -2f;
                Sounds.env_arrowToyChar.Play(base.X, 1f);
                return true;
            }
        }
        return false;
    }

    protected override void CreateGraphics()
    {
        var self = DynamicData.For(this);
        var normalSprite = new Sprite<int>(TFGame.Atlas["arrows/brambleArrow"], 13, 6);
        normalSprite.Origin = new Vector2(12, 3);
        normalSprite.OnAnimationComplete = (s) => {};

        var buriedSprite = new Sprite<int>(TFGame.Atlas["arrows/brambleArrowBuried"], 13, 6);
        buriedSprite.Origin = new Vector2(12, 3);
        var eyeballSprite = TFGame.SpriteData.GetSpriteInt("TriggerArrowEyeball");
        var pupilSprite = TFGame.SpriteData.GetSpriteInt("TriggerArrowPupil");
        eyeballSprite.Visible = false;


        this.Graphics = new Image[]
        {
            normalSprite,
            buriedSprite,
            eyeballSprite,
            pupilSprite
        };

        self.Set("normalSprite", normalSprite);
        self.Set("buriedSprite", buriedSprite);
        self.Set("eyeballSprite", eyeballSprite);
        self.Set("pupilSprite", pupilSprite);
        base.Add(this.Graphics);
    }

    public static void Load() 
    {
        BaseInit = CallHelper.CallBaseGen<Arrow, TriggerArrow, LevelEntity, Vector2, float>("Init", BindingFlags.NonPublic | BindingFlags.Instance);
        On.TowerFall.TriggerArrow.SetDetonator_Player += SetDetonatorPlayerPatch;
        On.TowerFall.TriggerArrow.SetDetonator_Enemy += SetDetonatorEnemyPatch;
        On.TowerFall.TriggerArrow.Detonate += DetonatePatch;
        On.TowerFall.TriggerArrow.Init += InitPatch;
        On.TowerFall.TriggerArrow.RemoveDetonator += RemoveDetonatorPatch;
    }

    private static void RemoveDetonatorPatch(On.TowerFall.TriggerArrow.orig_RemoveDetonator orig, TriggerArrow self)
    {
        if (self is TriggerBrambleArrow) 
        {
            self.LightVisible = false;
            var dynData = DynamicData.For(self);
            Player player = dynData.Get<Player>("playerDetonator");
            dynData.Set("playerDetonator", null);
            if (player != null) 
            {
                player.RemoveTriggerArrow(self);
            }
            dynData.Set("enemyDetonator", null);
            return;
        }
        orig(self);
    }

    private static void InitPatch(On.TowerFall.TriggerArrow.orig_Init orig, TriggerArrow self, LevelEntity owner, Vector2 position, float direction)
    {
        if (self is TriggerBrambleArrow bramble) 
        {
            BaseInit(self, owner, position, direction);
            Logger.Log("Working");
            self.LightVisible = true;
            Player playerDetonator = null;
            Enemy enemyDetonator = null;
            var dynData = DynamicData.For(self);
            dynData.Get<Alarm>("primed").Start();
            dynData.Get<Alarm>("enemyDetonateCheck").Stop();
            dynData.Set("playerDetonator", playerDetonator);
            dynData.Set("enemyDetonator", enemyDetonator);
            if (owner is Enemy)
            {
                bramble.SetDetonator(owner as Enemy);
            }

            bramble.used = bramble.canDie = false;
            bramble.StopFlashing();
            return;
        }
        orig(self, owner, position, direction);
    }

    public static void Unload() 
    {
        On.TowerFall.TriggerArrow.SetDetonator_Player -= SetDetonatorPlayerPatch;
        On.TowerFall.TriggerArrow.SetDetonator_Enemy -= SetDetonatorEnemyPatch;
        On.TowerFall.TriggerArrow.Detonate -= DetonatePatch;
        On.TowerFall.TriggerArrow.Init -= InitPatch;
        On.TowerFall.TriggerArrow.RemoveDetonator -= RemoveDetonatorPatch;
    }

    private static void DetonatePatch(On.TowerFall.TriggerArrow.orig_Detonate orig, TriggerArrow self)
    {
        if (self is TriggerBrambleArrow brambleSelf) 
        {
            DynamicData.For(self).Set("enemyDetonator", null);
            DynamicData.For(self).Set("playerDetonator", null);
            if (self.Scene != null && !self.MarkedForRemoval) 
            {
                brambleSelf.UseBramblePower();
            }
            return;
        }
        orig(self);
    }

    private static void SetDetonatorEnemyPatch(On.TowerFall.TriggerArrow.orig_SetDetonator_Enemy orig, TriggerArrow self, Enemy enemy)
    {
        if (self is TriggerBrambleArrow) 
        {
            DynamicData.For(self).Set("enemyDetonator", enemy);
            DynamicData.For(self).Get<Alarm>("enemyDetonateCheck").Start();
            return;
        }
        orig(self, enemy);
    }

    private static void SetDetonatorPlayerPatch(On.TowerFall.TriggerArrow.orig_SetDetonator_Player orig, TriggerArrow self, Player player)
    {
        if (self is TriggerBrambleArrow) 
        {
            DynamicData.For(self).Set("playerDetonator", player);
            return;
        }
        orig(self, player);
    }


    public override bool CanCatch(LevelEntity catcher)
    {
        return !used && base.CanCatch(catcher);
    }

    public void UseBramblePower()
    {
        this.used = true;
        Add(new Coroutine(
            Brambles.CreateBrambles(Level, Position, PlayerIndex, () => canDie = true)));
    }


    public override void Update()
    {
        base.Update();
        if (canDie) 
        {
            RemoveSelf();
        }
    }
}

public class PatchEnemyBramble 
{
    public static void Load() 
    {
        On.TowerFall.Enemy.Update += EnemyUpdatePatch;
    }

    private static void EnemyUpdatePatch(On.TowerFall.Enemy.orig_Update orig, Enemy self)
    {
        orig(self);
        self.CollideDo(GameTags.Brambles, e => {
            if (e is Brambles brambles)
            {
                if (brambles.OwnerIndex < 0)
                    return;
                self.Hurt(new Vector2(0, 0), 1, brambles.OwnerIndex);
            }
        });
    }

    public static void Unload() 
    {
        On.TowerFall.Enemy.Update -= EnemyUpdatePatch;
    }
}
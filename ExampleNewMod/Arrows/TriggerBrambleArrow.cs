using System.Reflection;
using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.NewExampleMod;

public class TriggerBrambleArrow : TriggerArrow
{
    // This is automatically been set by the mod loader
    public override ArrowTypes ArrowType { get; set; }
    private bool used, canDie;


    public TriggerBrambleArrow() : base()
    {
    }

    public static void Register(IHarmony harmony, IModContent content, IModRegistry registry)
    {
        var brambleArrow = registry.Subtextures.RegisterTexture(() => TFGame.Atlas["player/arrowHUD/brambleArrow"]);

        registry.Arrows.RegisterArrows("TriggerBrambleArrow", new()
        {
            ArrowType = typeof(TriggerBrambleArrow),
            HUD = brambleArrow
        });

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(TriggerArrow), nameof(TriggerArrow.SetDetonator), [typeof(Player)]),
            new HarmonyMethod(SetDetonatorPlayerPatch)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(TriggerArrow), nameof(TriggerArrow.SetDetonator), [typeof(Enemy)]),
            new HarmonyMethod(SetDetonatorEnemyPatch)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(TriggerArrow), nameof(Detonate)),
            new HarmonyMethod(DetonatePatch)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(TriggerArrow), nameof(RemoveDetonator)),
            new HarmonyMethod(RemoveDetonatorPatch)
        );
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

    private static bool RemoveDetonatorPatch(TriggerArrow __instance)
    {
        if (__instance is TriggerBrambleArrow) 
        {
            __instance.LightVisible = false;
            var dynData = DynamicData.For(__instance);
            Player? player = dynData.Get<Player>("playerDetonator");
            dynData.Set("playerDetonator", null);

            if (player != null) 
            {
                player.RemoveTriggerArrow(__instance);
            }
            dynData.Set("enemyDetonator", null);
            return false;
        }

        return true;
    }

    [MonoModLinkTo("TowerFall.Arrow", "System.Void Init(TowerFall.LevelEntity,Microsoft.Xna.Framework.Vector2,System.Single)")]
    protected void base_Init(LevelEntity owner, Vector2 position, float direction) 
    {
        base.Init(owner, position, direction);
    }

    protected override void Init(LevelEntity owner, Vector2 position, float direction)
    {
        base_Init(owner, position, direction);
        LightVisible = true;
        Player? playerDetonator = null;
        Enemy? enemyDetonator = null;
        var dynData = DynamicData.For(this);
        dynData.Get<Alarm>("primed")!.Start();
        dynData.Get<Alarm>("enemyDetonateCheck")!.Stop();
        dynData.Set("playerDetonator", playerDetonator);
        dynData.Set("enemyDetonator", enemyDetonator);
        if (owner is Enemy)
        {
            SetDetonator(owner as Enemy);
        }

        used = canDie = false;
        StopFlashing();
    }

    // private static void InitPatch(On.TowerFall.TriggerArrow.orig_Init orig, TriggerArrow self, LevelEntity owner, Vector2 position, float direction)
    // {
    //     if (self is TriggerBrambleArrow bramble) 
    //     {
    //         BaseInit(self, owner, position, direction);
    //         self.LightVisible = true;
    //         Player playerDetonator = null;
    //         Enemy enemyDetonator = null;
    //         var dynData = DynamicData.For(self);
    //         dynData.Get<Alarm>("primed").Start();
    //         dynData.Get<Alarm>("enemyDetonateCheck").Stop();
    //         dynData.Set("playerDetonator", playerDetonator);
    //         dynData.Set("enemyDetonator", enemyDetonator);
    //         if (owner is Enemy)
    //         {
    //             bramble.SetDetonator(owner as Enemy);
    //         }

    //         bramble.used = bramble.canDie = false;
    //         bramble.StopFlashing();
    //         return;
    //     }
    //     orig(self, owner, position, direction);
    // }


    private static bool DetonatePatch(TriggerArrow __instance)
    {
        if (__instance is TriggerBrambleArrow brambleSelf) 
        {
            DynamicData.For(__instance).Set("enemyDetonator", null);
            DynamicData.For(__instance).Set("playerDetonator", null);
            if (__instance.Scene != null && !__instance.MarkedForRemoval) 
            {
                brambleSelf.UseBramblePower();
            }
            return false;
        }
        return true;
    }

    private static bool SetDetonatorEnemyPatch(TriggerArrow __instance, Enemy enemy)
    {
        if (__instance is TriggerBrambleArrow) 
        {
            DynamicData.For(__instance).Set("enemyDetonator", enemy);
            DynamicData.For(__instance).Get<Alarm>("enemyDetonateCheck")!.Start();
            return false;
        }
        return true;
    }

    private static bool SetDetonatorPlayerPatch(TriggerArrow __instance, Player player)
    {
        if (__instance is TriggerBrambleArrow) 
        {
            DynamicData.For(__instance).Set("playerDetonator", player);
            return false;
        }

        return true;
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
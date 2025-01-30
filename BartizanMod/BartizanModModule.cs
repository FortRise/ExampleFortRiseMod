using System;
using System.Reflection;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using MonoMod.ModInterop;
using MonoMod.Utils;
using TowerFall;

namespace BartizanMod;


[Fort("com.kha.BartizanMod", "BartizanMod")]
public class BartizanModModule : FortModule
{
    public static Atlas BartizanAtlas;
    public static SpriteData BartizanData;
    public static BartizanModModule Instance;

    public override Type SettingsType => typeof(BartizanModSettings);
    public BartizanModSettings Settings => (BartizanModSettings)Instance.InternalSettings;

    public static bool EightPlayerMod;

    public BartizanModModule() 
    {
        Instance = this;
    }

    public override void LoadContent()
    {
        BartizanAtlas = Content.LoadAtlas("Atlas/atlas.xml", "Atlas/atlas.png");
        BartizanData = Content.LoadSpriteData("Atlas/SpriteData/spriteData.xml", BartizanAtlas);
    }

    public override void OnVariantsRegister(VariantManager manager, bool noPerPlayer = false) 
    {
        var info = new CustomVariantInfo(
                            // You can directly use Subtexture from your atlas
            "NoHeadBounce", VariantManager.GetVariantIconFromName("NoHeadBounce", BartizanAtlas), 
            CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom);
        var noHeadBounce = manager.AddVariant(info, noPerPlayer);
        var noDodgeCooldown = manager.AddVariant(DeclareFromInfo("NoDodgeCooldowns"), noPerPlayer);
        var awfullyFastArrows = manager.AddVariant(DeclareFromInfo("AwfullyFastArrows"), noPerPlayer);
        var awfullySlowArrows = manager.AddVariant(DeclareFromInfo("AwfullySlowArrows"), noPerPlayer);
        var noLedgeGrab = manager.AddVariant(DeclareFromInfo("NoLedgeGrab"), noPerPlayer);
        var infiniteArrows = manager.AddVariant(DeclareFromInfo("InfiniteArrows"), noPerPlayer);
        
        manager.CreateLinks(manager.MatchVariants.ShowDodgeCooldown, noDodgeCooldown);
        manager.CreateLinks(awfullyFastArrows, awfullySlowArrows);

        CustomVariantInfo DeclareFromInfo(string name) 
        {
            return info with {
                Name = name,
                Icon = VariantManager.GetVariantIconFromName(name, BartizanAtlas)
            };
        }
    }

    public override void Load()
    {
        typeof(EightPlayerImport).ModInterop();
        RespawnRoundLogic.Load();
        MyPlayerGhost.Load();
        MyRollcallElement.Load();
        MyVersusPlayerMatchResults.Load();
        MyPlayer.Load();
        MyArrow.Load();
        MyPlayerCorpse.Load();
    }

    public override void Unload()
    {
        RespawnRoundLogic.Unload();
        MyPlayerGhost.Unload();
        MyRollcallElement.Unload();
        MyVersusPlayerMatchResults.Unload();
        MyPlayer.Unload();
        MyArrow.Unload();
        MyPlayerCorpse.Unload();
    }

    public override void Initialize()
    {
        EightPlayerMod = IsModExists("WiderSetMod");
    }
}

// [CustomPickup("Bartizan/TeleporterOrb", Chance = 0.5f)]
// public class TeleporterOrb : CustomOrbPickup 
// {
//     public TeleporterOrb(Vector2 position, Vector2 targetPosition) : base(position, targetPosition)
//     {
//     }

//     public override void Collect(Player player)
//     {
//         Sounds.pu_darkOrbCollect.Play(base.X, 1f);
//         player.Position.X = WrapMath.ApplyWrapX(Position.X - (Position.X * 2));
//         base.Level.Particles.Emit(Particles.DarkOrbCollect, 12, this.Position, Vector2.One * 4f);

//         ShockCircle shockCircle = Cache.Create<ShockCircle>();
//         shockCircle.Init(player.Position, player.PlayerIndex, player, ShockCircle.ShockTypes.TeamRevive);
//         base.Level.Add<ShockCircle>(shockCircle);

//         player.Flash(35, null);
//         Sounds.sfx_reviveBlueteamFinish.Play(160f, 1f);
//     }

//     public override CustomOrbInfo CreateInfo()
//     {
//         var sprite = BartizanModModule.BartizanData.GetSpriteInt("TeleporterOrb");
//         return new CustomOrbInfo(
//             new Hitbox(16, 16, -8, -8), Color.Aqua, sprite);
//     }
// }


// [CustomArrows("Bartizan/TriggerBramble", "CreateGraphicPickup")]
// public class TriggerBrambleArrow : TriggerArrow
// {
//     // This is automatically been set by the mod loader
//     public override ArrowTypes ArrowType { get; set; }
//     private bool used, canDie;


//     public static ArrowInfo CreateGraphicPickup() 
//     {
//         var graphic = new Sprite<int>(TFGame.Atlas["pickups/bombArrows"], 12, 12, 0);
//         graphic.Add(0, 0.3f, new int[2] { 0, 1 });
//         graphic.Play(0, false);
//         graphic.CenterOrigin();
//         var arrowInfo = ArrowInfo.Create(graphic, TFGame.Atlas["player/arrowHUD/brambleArrow"]);
//         arrowInfo.Name = "TriggerBramble Arrows";
//         return arrowInfo;
//     }

//     public TriggerBrambleArrow() : base()
//     {
//     }

//     protected override bool CheckForTargetCollisions()
//     {
//         foreach (Entity entity in base.Level[GameTags.Target])
//         {
//             var levelEntity = (LevelEntity)entity;
//             if (levelEntity.ArrowCheck(this) && levelEntity != this.CannotHit)
//             {
//                 Vector2 vector = (levelEntity.Position - (this.Position - this.Speed)).SafeNormalize();
//                 levelEntity.OnSqueakyBounce(this, vector);
//                 base.State = Arrow.ArrowStates.Falling;
//                 this.Speed = vector * -2f;
//                 Sounds.env_arrowToyChar.Play(base.X, 1f);
//                 return true;
//             }
//         }
//         return false;
//     }

//     protected override void CreateGraphics()
//     {
//         var self = DynamicData.For(this);
//         var normalSprite = new Sprite<int>(TFGame.Atlas["arrows/brambleArrow"], 13, 6);
//         normalSprite.Origin = new Vector2(12, 3);
//         normalSprite.OnAnimationComplete = (s) => {};

//         var buriedSprite = new Sprite<int>(TFGame.Atlas["arrows/brambleArrowBuried"], 13, 6);
//         buriedSprite.Origin = new Vector2(12, 3);
//         var eyeballSprite = TFGame.SpriteData.GetSpriteInt("TriggerArrowEyeball");
//         var pupilSprite = TFGame.SpriteData.GetSpriteInt("TriggerArrowPupil");
//         eyeballSprite.Visible = false;


//         this.Graphics = new Image[]
//         {
//             normalSprite,
//             buriedSprite,
//             eyeballSprite,
//             pupilSprite
//         };

//         self.Set("normalSprite", normalSprite);
//         self.Set("buriedSprite", buriedSprite);
//         self.Set("eyeballSprite", eyeballSprite);
//         self.Set("pupilSprite", pupilSprite);
//         base.Add(this.Graphics);
//     }

//     public static void Load() 
//     {
//         On.TowerFall.TriggerArrow.SetDetonator_Player += SetDetonatorPlayerPatch;
//         On.TowerFall.TriggerArrow.SetDetonator_Enemy += SetDetonatorEnemyPatch;
//         On.TowerFall.TriggerArrow.Detonate += DetonatePatch;
//         On.TowerFall.TriggerArrow.RemoveDetonator += RemoveDetonatorPatch;
//     }

//     public static void Unload() 
//     {
//         On.TowerFall.TriggerArrow.SetDetonator_Player -= SetDetonatorPlayerPatch;
//         On.TowerFall.TriggerArrow.SetDetonator_Enemy -= SetDetonatorEnemyPatch;
//         On.TowerFall.TriggerArrow.Detonate -= DetonatePatch;
//         On.TowerFall.TriggerArrow.RemoveDetonator -= RemoveDetonatorPatch;
//     }

//     private static void RemoveDetonatorPatch(On.TowerFall.TriggerArrow.orig_RemoveDetonator orig, TriggerArrow self)
//     {
//         if (self is TriggerBrambleArrow) 
//         {
//             self.LightVisible = false;
//             var dynData = DynamicData.For(self);
//             Player player = dynData.Get<Player>("playerDetonator");
//             dynData.Set("playerDetonator", null);
//             if (player != null) 
//             {
//                 player.RemoveTriggerArrow(self);
//             }
//             dynData.Set("enemyDetonator", null);
//             return;
//         }
//         orig(self);
//     }

//     [MonoModLinkTo("TowerFall.Arrow", "System.Void Init(TowerFall.LevelEntity,Microsoft.Xna.Framework.Vector2,System.Single)")]
//     protected void base_Init(LevelEntity owner, Vector2 position, float direction) 
//     {
//         base.Init(owner, position, direction);
//     }

//     protected override void Init(LevelEntity owner, Vector2 position, float direction)
//     {
//         base_Init(owner, position, direction);
//         LightVisible = true;
//         Player playerDetonator = null;
//         Enemy enemyDetonator = null;
//         var dynData = DynamicData.For(this);
//         dynData.Get<Alarm>("primed").Start();
//         dynData.Get<Alarm>("enemyDetonateCheck").Stop();
//         dynData.Set("playerDetonator", playerDetonator);
//         dynData.Set("enemyDetonator", enemyDetonator);
//         if (owner is Enemy)
//         {
//             SetDetonator(owner as Enemy);
//         }

//         used = canDie = false;
//         StopFlashing();
//     }

//     // private static void InitPatch(On.TowerFall.TriggerArrow.orig_Init orig, TriggerArrow self, LevelEntity owner, Vector2 position, float direction)
//     // {
//     //     if (self is TriggerBrambleArrow bramble) 
//     //     {
//     //         BaseInit(self, owner, position, direction);
//     //         self.LightVisible = true;
//     //         Player playerDetonator = null;
//     //         Enemy enemyDetonator = null;
//     //         var dynData = DynamicData.For(self);
//     //         dynData.Get<Alarm>("primed").Start();
//     //         dynData.Get<Alarm>("enemyDetonateCheck").Stop();
//     //         dynData.Set("playerDetonator", playerDetonator);
//     //         dynData.Set("enemyDetonator", enemyDetonator);
//     //         if (owner is Enemy)
//     //         {
//     //             bramble.SetDetonator(owner as Enemy);
//     //         }

//     //         bramble.used = bramble.canDie = false;
//     //         bramble.StopFlashing();
//     //         return;
//     //     }
//     //     orig(self, owner, position, direction);
//     // }


//     private static void DetonatePatch(On.TowerFall.TriggerArrow.orig_Detonate orig, TriggerArrow self)
//     {
//         if (self is TriggerBrambleArrow brambleSelf) 
//         {
//             DynamicData.For(self).Set("enemyDetonator", null);
//             DynamicData.For(self).Set("playerDetonator", null);
//             if (self.Scene != null && !self.MarkedForRemoval) 
//             {
//                 brambleSelf.UseBramblePower();
//             }
//             return;
//         }
//         orig(self);
//     }

//     private static void SetDetonatorEnemyPatch(On.TowerFall.TriggerArrow.orig_SetDetonator_Enemy orig, TriggerArrow self, Enemy enemy)
//     {
//         if (self is TriggerBrambleArrow) 
//         {
//             DynamicData.For(self).Set("enemyDetonator", enemy);
//             DynamicData.For(self).Get<Alarm>("enemyDetonateCheck").Start();
//             return;
//         }
//         orig(self, enemy);
//     }

//     private static void SetDetonatorPlayerPatch(On.TowerFall.TriggerArrow.orig_SetDetonator_Player orig, TriggerArrow self, Player player)
//     {
//         if (self is TriggerBrambleArrow) 
//         {
//             DynamicData.For(self).Set("playerDetonator", player);
//             return;
//         }
//         orig(self, player);
//     }


//     public override bool CanCatch(LevelEntity catcher)
//     {
//         return !used && base.CanCatch(catcher);
//     }

//     public void UseBramblePower()
//     {
//         this.used = true;
//         Add(new Coroutine(
//             Brambles.CreateBrambles(Level, Position, PlayerIndex, () => canDie = true)));
//     }


//     public override void Update()
//     {
//         base.Update();
//         if (canDie) 
//         {
//             RemoveSelf();
//         }
//     }
// }
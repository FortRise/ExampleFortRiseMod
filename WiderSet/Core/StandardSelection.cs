using System.Collections.Generic;
using FortRise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using TowerFall;

namespace Teuria.WiderSet;

internal class StandardSelectionFromVersus : CustomMenuState
{
    public StandardSelectionFromVersus(MainMenu main) : base(main)
    {
    }

    public override void Create()
    {
        LevelSetType levelSetType = LevelSetType.Versus;

        var standardSet = new StandardSetButton(new Vector2(160f - 60, 90f), new Vector2(-160f, 120f), levelSetType);
        var wideSet = new WideSetButton(new Vector2(160f + 60, 90f), new Vector2(560f, 120f), levelSetType);

        List<MenuItem> list = [standardSet, wideSet];

        Main.Add(list);
        standardSet.RightItem = wideSet;
        wideSet.LeftItem = standardSet;
        Main.BackState = MainMenu.MenuState.Main;
        Main.ToStartSelected = standardSet;
        Main.TweenBGCameraToY(1);
        Main.Add(new LevelSetDisplay(standardSet, wideSet, levelSetType));

    }

    public override void Destroy()
    {
    }
}

public enum LevelSetType { Versus, Coop }


internal sealed class StandardSetButton : MainModeButton
{
    private Sprite<int> tower;
    private LevelSetType levelSetType;
    public override bool Rotate => false;

    public StandardSetButton(Vector2 position, Vector2 tweenFrom, LevelSetType levelSetType) : base(
        position, tweenFrom, "STANDARD", levelSetType == LevelSetType.Coop ? "1-4 ARCHERS" : "2-4 ARCHERS")
    {
        this.levelSetType = levelSetType;
        tower = ((ISpriteEntry<int>)WiderSetModule.StandardSetSprite.Entry).Sprite!;
        tower.Play(0);
        tower.CenterOrigin();
        Add(tower);
    }

    public override float BaseScale => 1f;

    public override float ImageScale { get => tower.Scale.X; set => tower.Scale = Vector2.One * value; }
    public override float ImageRotation { get => tower.Rotation; set => tower.Rotation = value; }
    public override float ImageY { get => tower.Y; set => tower.Y = value; }

    protected override void OnSelect()
    {
        tower.Play(1);
        base.OnSelect();
    }

    protected override void OnDeselect()
    {
        tower.Play(0);
        base.OnDeselect();
    }

    protected override void OnConfirm()
    {
        base.OnConfirm();
        tower.Play(1);
        WiderSetModule.IsWide = false;
        // MainMenu.VersusMatchSettings.Teams = EightPlayerModule.StandardTeams;
    }

    protected override void MenuAction()
    {
        tower.Play(1);
        if (levelSetType == LevelSetType.Coop)
        {
            MainMenu.State = MainMenu.MenuState.CoOp;
            MainMenu.BackState = MainMenu.MenuState.Main;
            return;
        }
        MainMenu.RollcallMode = MainMenu.RollcallModes.Versus;
        MainMenu.State = MainMenu.MenuState.Rollcall;
        MainMenu.BackState = MainMenu.MenuState.Main;
    }
}

public class WideSetButton : MainModeButton
{
    private Sprite<int> tower;
    private LevelSetType levelSetType;
    public override bool Rotate => false;

    public WideSetButton(Vector2 position, Vector2 tweenFrom, LevelSetType levelSetType) : base(
        position, tweenFrom, "WIDER", levelSetType == LevelSetType.Coop ? "1-8 ARCHERS" : "2-8 ARCHERS")
    {
        this.levelSetType = levelSetType;
        tower = ((ISpriteEntry<int>)WiderSetModule.WideSetSprite.Entry).Sprite!;
        tower.Play(0);
        tower.CenterOrigin();
        Add(tower);
    }

    public override float BaseScale => 1f;

    public override float ImageScale { get => tower.Scale.X; set => tower.Scale = Vector2.One * value; }
    public override float ImageRotation { get => tower.Rotation; set => tower.Rotation = value; }
    public override float ImageY { get => tower.Y; set => tower.Y = value; }

    protected override void OnSelect()
    {
        tower.Play(1);
        base.OnSelect();
    }

    protected override void OnDeselect()
    {
        tower.Play(0);
        base.OnDeselect();
    }

    protected override void OnConfirm()
    {
        base.OnConfirm();
        tower.Play(1);
        WiderSetModule.IsWide = true;
        // MainMenu.VersusMatchSettings.Teams = EightPlayerModule.EightPlayerTeams;
    }

    protected override void MenuAction()
    {
        if (levelSetType == LevelSetType.Coop) 
        {
            MainMenu.State = MainMenu.MenuState.CoOp;
            MainMenu.BackState = MainMenu.MenuState.Main;
            return;
        }
        MainMenu.RollcallMode = MainMenu.RollcallModes.Versus;
        MainMenu.State = MainMenu.MenuState.Rollcall;
        MainMenu.BackState = MainMenu.MenuState.Main;
    }
}

internal class LevelSetDisplay : MenuItem
{
    private static Color WideTextColor = Calc.HexToColor("95F94D");
    private StandardSetButton standard;
    private WideSetButton eightPlayer;
    private bool standardSelected;
    private bool wideSelected;
    private float drawStandard;
    private float drawWide;
    private bool tweeningOut;
    private SineWave alphaSine;
    private LevelSetType setType;

    public LevelSetDisplay(StandardSetButton standardSet, WideSetButton eightPlayerSet, LevelSetType setType) : base(Vector2.Zero)
    {
        this.setType = setType;
        standard = standardSet;
        eightPlayer = eightPlayerSet;
        LayerIndex = -1;
        Add(alphaSine = new SineWave(120));
    }

    public override void Update()
    {
        base.Update();
        if (standard.Selected) 
        {
            standardSelected = true;
            wideSelected = false;
        }
        else if (eightPlayer.Selected) 
        {
            standardSelected = false;
            wideSelected = true;
        }
        if (tweeningOut)
        {
            drawStandard = Calc.Approach(drawWide, 0f, 0.05f * Engine.TimeMult);
            drawWide = Calc.Approach(drawWide, 0f, 0.05f * Engine.TimeMult);
        }
        else if (standardSelected)
        {
            drawStandard = Calc.Approach(drawStandard, 1f, 0.05f * Engine.TimeMult);
            drawWide = Calc.Approach(drawWide, 0f, 0.05f * Engine.TimeMult);
        }
        else if (wideSelected)
        {
            drawStandard = Calc.Approach(drawStandard, 0f, 0.05f * Engine.TimeMult);
            drawWide = Calc.Approach(drawWide, 1f, 0.05f * Engine.TimeMult);
        }
    }

    public override void Render()
    {
        base.Render();
        if (drawStandard > 0) 
        {
            Vector2 vector = Vector2.Lerp(Vector2.UnitX * -160f, Vector2.UnitX * 160f, Ease.CubeInOut(drawStandard));
            Draw.TextureBannerV(
                TFGame.MenuAtlas["questResults/tipBanner"], vector + new Vector2(0f, 185f), 
                new Vector2(100f, 37f), Vector2.One, 0f, Color.White * drawStandard, 
                SpriteEffects.None, base.Scene.FrameCounter * 0.03f, 4f, 5, 0.3926991f);
            if (setType == LevelSetType.Coop) 
            {
                Draw.OutlineTextCentered(TFGame.Font, "1-4 ARCHERS ONLY", vector + new Vector2(0f, 160f), QuestDifficultySelect.LegendaryColor, Color.Black);
                Draw.OutlineTextCentered(TFGame.Font, "ORIGINAL AND BALANCED LEVELS!", vector + new Vector2(0f, 168f), Color.White, Color.Black);
                Draw.OutlineTextCentered(TFGame.Font, "PERFECT SUITE FOR SOLO PLAYERS", vector + new Vector2(0f, 176f), Color.White, Color.Black);
            }
            else 
            {
                Draw.OutlineTextCentered(TFGame.Font, "2-4 ARCHERS ONLY", vector + new Vector2(0f, 160f), QuestDifficultySelect.LegendaryColor, Color.Black);
                Draw.OutlineTextCentered(TFGame.Font, "THE ORIGINAL TOWERFALL STAGES!", vector + new Vector2(0f, 168f), Color.White, Color.Black);
                Draw.OutlineTextCentered(TFGame.Font, "CLASSIC, FRANTIC FUN.", vector + new Vector2(0f, 176f), Color.White, Color.Black);
            }
        }
        if (drawWide > 0) 
        {
            Vector2 vector2 = Vector2.Lerp(Vector2.UnitX * 480f, Vector2.UnitX * 160f, Ease.CubeInOut(drawWide));
            Draw.TextureBannerV(TFGame.MenuAtlas["questResults/darkWorldBanner"], vector2 + new Vector2(0f, 185f), new Vector2(100f, 37f), Vector2.One, 0f, Color.White * drawWide, SpriteEffects.None, base.Scene.FrameCounter * 0.03f, 4f, 5, 0.3926991f);
            if (setType == LevelSetType.Coop) 
            {
                Draw.OutlineTextCentered(TFGame.Font, "SUPPORTS UP TO 4-8 ARCHERS", vector2 + new Vector2(0f, 160f), QuestDifficultySelect.LegendaryColor, Color.Black);
                Draw.OutlineTextCentered(TFGame.Font, "CHAOTIC AND FUN LEVELS", vector2 + new Vector2(0f, 168f), Color.White, Color.Black);
                Draw.OutlineTextCentered(TFGame.Font, "WIDER EXPERIENCE!", vector2 + new Vector2(0f, 176f), Color.White, Color.Black);
            }
            else 
            {
                Draw.OutlineTextCentered(TFGame.Font, "SUPPORTS UP TO 8 ARCHERS", vector2 + new Vector2(0f, 160f), WideTextColor, Color.Black);
                Draw.OutlineTextCentered(TFGame.Font, "EXPANDED STAGES FOR MORE FIGHTERS", vector2 + new Vector2(0f, 168f), Color.White, Color.Black);
                Draw.OutlineTextCentered(TFGame.Font, "AND EPIC TEAM BATTLES!", vector2 + new Vector2(0f, 176f), Color.White, Color.Black);
            }
        }
    }

    public override void TweenIn()
    {
        // Clear the frame cache to fix the broken recording issue.
        ReplayRecorder.ClearFrameCache();
    }

    public override void TweenOut()
    {
        tweeningOut = true;
    }

    protected override void OnConfirm()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnDeselect()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnSelect()
    {
        throw new System.NotImplementedException();
    }
}
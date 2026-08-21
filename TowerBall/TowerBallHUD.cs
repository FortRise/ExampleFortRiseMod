using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;
using System;
namespace TowerBall;

public class TowerBallHUD : Entity
{
	private TowerBallRoundLogic roundLogic;
	private Image teamA;
	private Image teamB;
	private Color colorA;
	private Color colorB;
    private Color colorC;
    public float FlashAlpha;

	public TowerBallHUD(TowerBallRoundLogic rL)
		: base(3)
	{
		roundLogic = rL;
		teamA = new OutlineImage(TFGame.MenuAtlas["teamA"]);
		teamB = new OutlineImage(TFGame.MenuAtlas["teamB"]);
		teamA.Y = teamB.Y = 0f;
		teamA.X = 5f - TowerBallModModule.TestHalfWideUI;
        teamB.X = 282f + TowerBallModModule.TestHalfWideUI;
		colorA = new Color(0, 64, 88);
		colorB = new Color(136, 20, 0);
		colorC = new Color(255, 255, 90);
        FlashAlpha = 0f;
	}

	public override void Update()
	{
		if (FlashAlpha > 0f)
		{
			FlashAlpha -= 0.04f * Engine.TimeMult;
		}
		base.Update();
	}

	public override void Render()
	{
		if (FlashAlpha > 0f && !SaveData.Instance.Options.RemoveScreenFlashEffects)
		{
			Draw.Rect(-TowerBallModModule.TestHalfWideUI, 0f, 320f + TowerBallModModule.TestWideUI, 240f, Color.White * FlashAlpha);
		}

		teamA.Render();
		teamB.Render();
		Draw.Rect(38f - TowerBallModModule.TestHalfWideUI, 5f, 34f, 20f, Color.Black);
		Draw.Rect(248f + TowerBallModModule.TestHalfWideUI, 5f, 34f, 20f, Color.Black);
		Draw.Rect(39f - TowerBallModModule.TestHalfWideUI, 6f, 32f, 18f, Color.White);
		Draw.Rect(249f + TowerBallModModule.TestHalfWideUI, 6f, 32f, 18f, Color.White);
		Draw.Rect(40f - TowerBallModModule.TestHalfWideUI, 7f, 30f, 16f, colorA);
		Draw.Rect(250f + TowerBallModModule.TestHalfWideUI, 7f, 30f, 16f, colorB);
        if (roundLogic.timed)
        {
            Draw.Rect(131f, 5f, 54f, 20f, Color.Black);
            Draw.Rect(132f, 6f, 52f, 18f, Color.White);
            Draw.Rect(133f, 7f, 50f, 16f, colorC);
            TimeSpan time = TimeSpan.FromSeconds(roundLogic.secondsleft);
            Draw.OutlineTextCentered(TFGame.Font, time.ToString(@"mm\:ss"), new Vector2(159f, 16f), Color.White, 2f);
            if (roundLogic.overtime)
            {
                Draw.OutlineTextCentered(TFGame.Font, "OVERTIME!", new Vector2(159f, 40f), Color.White, 2f);
            }
        }
        
        Draw.OutlineTextCentered(TFGame.Font, roundLogic.Session.Scores[0].ToString(), new Vector2(56f - TowerBallModModule.TestHalfWideUI, 16f), Color.White, 2f);
		Draw.OutlineTextCentered(TFGame.Font, roundLogic.Session.Scores[1].ToString(), new Vector2(266f + TowerBallModModule.TestHalfWideUI, 16f), Color.White, 2f);
	}
}

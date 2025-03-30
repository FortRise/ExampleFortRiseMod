using BaronMode.Interop;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace BaronMode.GameModes;

public class Baron : CustomGameMode
{
    private int[] totalLives = null!;

    public override void StartGame(Session session) 
    {
        var playerCount = EightPlayerUtils.GetMenuPlayerCount();
        totalLives = new int[playerCount];
        var goal = session.MatchSettings.GoalScore;

        for (int i = 0; i < playerCount; i++) 
        {
            if (TFGame.Players[i]) 
            {
                session.Scores[i] = goal;
                session.OldScores[i] = goal;
                totalLives[i] = goal;
            }
            else    
            {
                totalLives[i] = -1;    
            }
        }
    }

    public override RoundLogic CreateRoundLogic(Session session)
    {
        return new BaronRoundLogic(session, totalLives);
    }

    public override void Initialize()
    {
        Icon = TFGame.Atlas["BaronMode/gameModes/baron"];
        NameColor = Color.LightPink;
        CoinOffset = 12;
    }

    public override void InitializeSounds() {}

    public override Sprite<int> CoinSprite()
    {
        var sprite = new Sprite<int>(TFGame.Atlas["BaronMode/pickups/gemCoin"], 12, 10);

        sprite.Add(0, 0.1f, new int[] { 0, 0, 0, 1, 2, 3, 4, 5, 6, 7});
        sprite.Play(0);
        sprite.CenterOrigin();
        return sprite;
    }
}

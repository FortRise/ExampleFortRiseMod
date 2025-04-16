using Teuria.BaronMode.Interop;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.BaronMode.GameModes;

public class Baron : IVersusGameMode, IRegisterable
{
    public static void Register(IModRegistry registry)
    {
        registry.GameModes.RegisterVersusGameMode(new Baron());
    }

    private int[] totalLives = null!;
    public string Name => "Baron";
    public Color NameColor => Color.LightPink;

    public Subtexture Icon => TFGame.Atlas["Teuria.BaronMode/gameModes/baron"];

    public bool IsTeamMode => false;

    public void OnStartGame(Session session) 
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

    public RoundLogic OnCreateRoundLogic(Session session)
    {
        return new BaronRoundLogic(session, totalLives);
    }

    public Sprite<int> OverrideCoinSprite(Session session)
    {
        var sprite = new Sprite<int>(TFGame.Atlas["Teuria.BaronMode/pickups/gemCoin"], 12, 10);

        sprite.Add(0, 0.1f, [0, 0, 0, 1, 2, 3, 4, 5, 6, 7]);
        sprite.Play(0);
        sprite.CenterOrigin();
        return sprite;
    }

    public int OverrideCoinOffset(Session? session)
    {
        return 12;
    }
}

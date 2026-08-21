using FortRise;
using TowerFall;

namespace TowerBall;

public class TowerBall : IVersusGameMode
{
    public static IVersusGameModeEntry TowerBallEntry { get; private set; } = null!;

    public static void Register(IModuleContext context)
    {
        TowerBallEntry = context.Registry.GameModes.RegisterVersusGameMode(
            new TowerBall()
        );
    }

    public string Name => "TowerBall";

    public ISubtextureEntry Icon => TowerBallModModule.GameModeIcon;

    public bool IsTeamMode => true;

    public RoundLogic OnCreateRoundLogic(Session session)
    {
        return new TowerBallRoundLogic(session);
    }

    public void OnStartGame(Session session)
    {
    }

    public int GetMinimumTeamPlayers(MatchSettings matchSettings)
    {
        return 2;
    }
}


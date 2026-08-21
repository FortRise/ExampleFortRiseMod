using Monocle;
using TowerFall;

namespace TowerBall;

public class PlayerRespawner(int playerIndex, Allegiance team, TowerBallRoundLogic roundLogic)
{
	public int PlayerIndex = playerIndex;
	public Allegiance Team = team;
	public TowerBallRoundLogic RoundLogic = roundLogic;
	public Counter Alarm = new Counter(180);

    public bool Update()
	{
		Alarm.Update();
		if (!Alarm)
		{
			RoundLogic.RespawnPlayer(PlayerIndex, Team);
			return true;
		}
		return false;
	}
}

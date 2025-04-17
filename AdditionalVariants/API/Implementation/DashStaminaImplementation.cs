using System.Linq;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.AdditionalVariants;

internal partial class ApiImplementation
{
    internal class DashStaminaImplementation : IAdditionalVariantsAPI.IDashStaminaAPI
    {
        public bool HasEnoughGauge(Level level, int playerIndex)
        {
            foreach (Player player in level.Players.Cast<Player>())
            {
                if (player.PlayerIndex == playerIndex)
                {
                    DynamicData.For(player).TryGet<DashStamina>("dashStamina", out var stamina);
                    if (stamina is null)
                    {
                        return true;
                    }
                    return stamina.CanUseStamina;
                }
            }

            return true;
        }
    }
}

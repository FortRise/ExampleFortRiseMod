using TowerFall;

namespace Teuria.AdditionalVariants;

public partial interface IAdditionalVariantsAPI 
{
    /// <summary>
    /// Contains API for Dash Stamina variants.
    /// </summary>
    public interface IDashStaminaAPI
    {
        /// <summary>
        /// Checks if the stamina bar has enough gauge to dash.
        /// </summary>
        /// <param name="level">A <see cref="TowerFall.Level"/> scene</param>
        /// <param name="playerIndex">A player index to check</param>
        /// <returns>true if it has enough gauge on stamina bar</returns>
        public bool HasEnoughGauge(Level level, int playerIndex);
    }
}

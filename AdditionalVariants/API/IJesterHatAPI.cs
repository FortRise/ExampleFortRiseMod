using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TowerFall;

namespace Teuria.AdditionalVariants;

public partial interface IAdditionalVariantsAPI 
{
    public interface IAdditionalVariantsAPIHook;
    /// <summary>
    /// Contains API for Jester's Hat variants.
    /// </summary>
    public interface IJesterHatAPI 
    {
        /// <summary>
        /// Register hooks to modify the behavior of a certain actions of Jester's Hat variant.
        /// </summary>
        /// <param name="hooks">Hook that is implemented by the mod</param>
        /// <param name="priority">The priority for the hook. Higher priority hooks are called before the lower one.</param>
        public void RegisterHook(IHook hooks, int priority = 0);
        /// <summary>
        /// Unregister the given hook related to Jester's Hat variant.
        /// </summary>
        /// <param name="hook"></param>
        public void UnregisterHook(IHook hook);
        
        /// <summary>
        /// Contains a behavior that mod can modify and add on.
        /// </summary>
        public interface IHook : IAdditionalVariantsAPIHook
        {
            /// <summary>
            /// Modify the warp points where players would teleport after dashing.
            /// </summary>
            /// <param name="warpPoints">A list of nodes that player will be teleported</param>
            public void ModifyWarpPoints(IModifyWarpPointsArgs args) {}

            public void AfterTeleport(IAfterTeleportArgs args) {}

            public interface IModifyWarpPointsArgs
            {
                Player Player { get; }
                IList<Vector2> WarpPoints { get; }
            }

            public interface IAfterTeleportArgs 
            {
                Player Player { get; }
                Vector2 LastWarpPoint { get; }
            }
        }
    }
}

using FortRise;
using Microsoft.Xna.Framework;
using TowerFall;

namespace Teuria.Ascencore;

public partial interface IAscencoreAPI
{
    public IPlayerDodgeStateHookApi DodgeStateApi { get; }

    public interface IPlayerDodgeStateHookApi
    {
        public void RegisterHook(IHook hook, int priority = 0);

        public void UnregisterHook(IHook hook);


        public interface IHook
        {
            public Option<bool> IsDodgeEnabled(IsDodgeEnabledEventArgs dodgeStateArgs) => Option<bool>.None();
            public Option<Player.PlayerStates> DoOverrideEnterState(DoOverrideStateEventArgs doOverrideStateArgs) => Option<Player.PlayerStates>.None();
            public void OnLeaveDodge(OnLeaveDodgeEventArgs onLeaveDodgeEventArgs) {}

            public interface IsDodgeEnabledEventArgs
            {
                public Player Player { get; }
                public Player.PlayerStates State { get; }
            }

            public interface DoOverrideStateEventArgs
            {
                public Player Player { get; }
                public Player.PlayerStates State { get; }
            }

            public interface OnLeaveDodgeEventArgs
            {
                public Player Player { get; }
            }
        }
    }
}
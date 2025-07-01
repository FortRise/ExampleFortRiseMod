using TowerFall;
using System.Collections.Generic;
using HarmonyLib;

namespace Teuria.Ascencore;

internal sealed class PlayerDodgeStateHookApi : IAscencoreAPI.IPlayerDodgeStateHookApi
{
    public PlayerDodgeStateHookApi()
    {
    }

    public void RegisterHook(IAscencoreAPI.IPlayerDodgeStateHookApi.IHook hook, int priority = 0)
    {
        PlayerDodgeStateHookManager.Instance.AddHook(hook, priority);
    }

    public void UnregisterHook(IAscencoreAPI.IPlayerDodgeStateHookApi.IHook hook)
    {
        PlayerDodgeStateHookManager.Instance.RemoveHook(hook);
    }
}

internal sealed class PlayerDodgeStateOnLeaveDodgeEventArgs(Player player) 
    : IAscencoreAPI.IPlayerDodgeStateHookApi.IHook.OnLeaveDodgeEventArgs
{
    public Player Player { get; } = player;
}

internal sealed class PlayerDodgeStateEventArgs(Player player, Player.PlayerStates state) 
    : IAscencoreAPI.IPlayerDodgeStateHookApi.IHook.IsDodgeEnabledEventArgs,
    IAscencoreAPI.IPlayerDodgeStateHookApi.IHook.DoOverrideStateEventArgs
{
    public Player Player { get; } = player;
    public Player.PlayerStates State { get; } = state;
}

internal sealed class PlayerDodgeStateHookManager
{
    public static PlayerDodgeStateHookManager Instance { get; private set; } = null!;
    private readonly OrderedList<IAscencoreAPI.IPlayerDodgeStateHookApi.IHook> hooks = [];

    public IReadOnlyList<IAscencoreAPI.IPlayerDodgeStateHookApi.IHook> Hooks => hooks;

    public PlayerDodgeStateHookManager()
    {
        Instance = this;
        var harmony = AscencoreModule.Instance.Context.Harmony;

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), "NormalUpdate"),
            postfix: new HarmonyMethod(Player_NormalUpdate_Postfix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), "DuckingUpdate"),
            postfix: new HarmonyMethod(Player_DuckingUpdate_Postfix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), "DyingUpdate"),
            postfix: new HarmonyMethod(Player_DyingUpdate_Postfix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), "LedgeGrabUpdate"),
            postfix: new HarmonyMethod(Player_LedgeGrabUpdate_Postfix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), "LeaveDodge"),
            postfix: new HarmonyMethod(Player_LeaveDodge_Postfix)
        );
    }

    public void AddHook(IAscencoreAPI.IPlayerDodgeStateHookApi.IHook hook, int priority)
    {
        hooks.Add(hook, priority);
    }

    public void RemoveHook(IAscencoreAPI.IPlayerDodgeStateHookApi.IHook hook)
    {
        hooks.Remove(hook);
    }

    private static void Player_NormalUpdate_Postfix(Player __instance, ref int __result)
        => PlayerRunUpdate(Player.PlayerStates.Normal, __instance, ref __result);

    private static void Player_DuckingUpdate_Postfix(Player __instance, ref int __result)
        => PlayerRunUpdate(Player.PlayerStates.Ducking, __instance, ref __result);

    private static void Player_DyingUpdate_Postfix(Player __instance, ref int __result)
        => PlayerRunUpdate(Player.PlayerStates.Dying, __instance, ref __result);

    private static void Player_LedgeGrabUpdate_Postfix(Player __instance, ref int __result)
        => PlayerRunUpdate(Player.PlayerStates.LedgeGrab, __instance, ref __result);

    private static void Player_LeaveDodge_Postfix(Player __instance)
    {
        foreach (var hook in Instance.Hooks)
        {
            hook.OnLeaveDodge(new PlayerDodgeStateOnLeaveDodgeEventArgs(__instance));
        }
    }

    private static void PlayerRunUpdate(Player.PlayerStates state, Player __instance, ref int __result)
    {
        if (__result != 3)
        {
            return;
        }

        var eventArgs = new PlayerDodgeStateEventArgs(__instance, (Player.PlayerStates)__result);
        foreach (var hook in Instance.Hooks)
        {
            var dodgeEnable = hook.IsDodgeEnabled(eventArgs);
            if (dodgeEnable.TryGetValue(out bool dodgeEnabled) && !dodgeEnabled)
            {
                __result = (int)state;
                return;
            }

            var doOverrideState = hook.DoOverrideEnterState(eventArgs);
            if (doOverrideState.TryGetValue(out Player.PlayerStates s))
            {
                __result = (int)s;
            }
        }
    }
}
using System.Collections.Generic;
using HarmonyLib;
using TowerFall;

namespace Teuria.Ascencore;


internal sealed class ApiImplementation : IAscencoreAPI
{
    public IAscencoreAPI.IPlayerDodgeStateHookApi DodgeStateApi { get; } = new PlayerDodgeStateHookApi();
    public ApiImplementation()
    {
        _ = new PlayerDodgeStateHookManager();
    }
}


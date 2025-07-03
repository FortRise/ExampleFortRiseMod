using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FortRise;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class LevelSystemHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LevelSystem), nameof(LevelSystem.GetBackground)),
            postfix: new HarmonyMethod(LevelSystem_GetBackground_Postfix)
        );
    }

    private static void LevelSystem_GetBackground_Postfix(Level level, LevelSystem __instance, ref Background __result)
    {
        if (WiderSetModule.IsWide)
        {
            ref var val = ref CollectionsMarshal.GetValueRefOrNullRef(WiderSetModule.WideRedirector, __instance.Theme.ID);
            if (Unsafe.IsNullRef(ref val))
            {
                return;
            }

            var bg = new Background(level, WiderSetModule.WideBG[val]["Background"]);

            __result = bg;
        }
    }
}
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FortRise;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class UnsafeVersusLevelSystem
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "levels")]
    public static extern ref List<string> GetLevels(VersusLevelSystem instance);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_VersusTowerData")]
    public static extern void SetVersusTowerData(VersusLevelSystem instance, VersusTowerData? value);
}

internal sealed class VersusLevelSystemHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(VersusLevelSystem), [typeof(VersusTowerData)]),
            postfix: new HarmonyMethod(VersusLevelSystem_ctor_Postfix)
        );
    }

    private static void VersusLevelSystem_ctor_Postfix(VersusLevelSystem __instance)
    {
        if (!WiderSetModule.IsWide)
        {
            return;
        }

        string levelID = __instance.VersusTowerData.GetLevelID();

        if (__instance.VersusTowerData.IsOfficialLevelSet())
        {
            ref IVersusTowerEntry level = ref CollectionsMarshal.GetValueRefOrNullRef(
                WiderSetModule.MapEntry, 
                levelID);

            if (Unsafe.IsNullRef(ref level))
            {
                return;
            }

            UnsafeVersusLevelSystem.SetVersusTowerData(__instance, level.VersusTowerData);
            return;
        }
    }
}

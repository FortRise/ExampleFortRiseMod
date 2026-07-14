using System.Collections.Generic;
using System.Linq;
using FortRise;
using HarmonyLib;

namespace Teuria.WiderSet;

internal sealed class CustomLevelCategoryButtonHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(CustomLevelCategoryButton), "CreateLevelSets"),
            postfix: new HarmonyMethod(CreateLevelSets_Postfix)
        );
    }

    private static void CreateLevelSets_Postfix(ref List<string> __result)
    {
        if (WiderSetModule.IsWide)
        {
            __result = [.. __result.Where(x => x.Contains("<Teuria.WiderSet>"))];
        }
        else 
        {
            __result = [.. __result.Where(x => !x.Contains("<Teuria.WiderSet>"))];
        }
    }
}

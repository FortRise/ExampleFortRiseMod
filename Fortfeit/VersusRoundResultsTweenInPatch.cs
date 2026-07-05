using System.Linq;
using System.Reflection;
using HarmonyLib;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.Fortfeit;

[HarmonyPatch]
public static class VersusRoundResultsTweenInPatch
{
    public static MethodBase TargetMethod()
    {
        return typeof(VersusRoundResults).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).First(x => x.Name.Contains("<TweenIn>") && x.Name.Contains('2'));
    }

    public static void Postfix(VersusRoundResults __instance)
    {
        var forfeit = DynamicData.For(__instance).Get("Teuria.Forfeit::forfeit");

        if (forfeit is not bool isForfeit)
        {
            return;
        }

        if (isForfeit)
        {
            return;
        }

        if (TempLifetime.RoundReults.TryGetTarget(out var guide))
        {
            guide.Visible = true;
        }
    }
}

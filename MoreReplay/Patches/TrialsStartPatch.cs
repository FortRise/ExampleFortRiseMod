using FortRise;
using HarmonyLib;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.MoreReplay;

public sealed class TrialsStartPatch : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.EnumeratorMoveNext(
                AccessTools.DeclaredMethod(typeof(TrialsStart), "Sequence")),
            postfix: new HarmonyMethod(Sequence_MoveNext_Postfix)
        );

        harmony.Patch(
            AccessTools.EnumeratorMoveNext(
                AccessTools.DeclaredMethod(typeof(TrialsStart), "SkipSequence")),
            postfix: new HarmonyMethod(Sequence_MoveNext_Postfix)
        );
    }

    private static void Sequence_MoveNext_Postfix(object __instance, ref bool __result)
    {
        if (__result)
        {
            return;
        }

        if (SaveData.Instance.Options.ReplayMode == Options.ReplayModes.Off)
        {
            return;
        }

        var instance = Traverse.Create(__instance).Field<TrialsStart>("<>4__this").Value;
        var level = instance.Level;

        DynamicData.For(level).Invoke("set_ReplayRecorder", new ReplayRecorder(level));
    }
}


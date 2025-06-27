using FortRise;
using HarmonyLib;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.MoreReplay;

public class QuestGameOverPatch : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(QuestGameOver), [typeof(QuestRoundLogic)]),
            postfix: new HarmonyMethod(QuestGameOver_ctor_Postfix)
        );
    }

    private static void QuestGameOver_ctor_Postfix(QuestGameOver __instance)
    {
        var dynSelf = DynamicData.For(__instance);
        dynSelf.Set("replaySaved", false);
        dynSelf.Set("saving", false);

        var replayMenuComponent = new ReplayMenuComponent();
        __instance.Add(replayMenuComponent);
    }
}
using System.Xml;
using FortRise;
using HarmonyLib;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.MoreReplay;

public class LevelPatch : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(Level), [typeof(Session), typeof(XmlElement)]),
            postfix: new HarmonyMethod(Level_ctor_Postfix)
        );
    }

    private static void Level_ctor_Postfix(TowerFall.Level __instance, TowerFall.Session session)
    {
        var dynSelf = DynamicData.For(__instance);
        if (session.MatchSettings.SoloMode && SaveData.Instance.Options.ReplayMode != Options.ReplayModes.Off)
        {
            dynSelf.Invoke("set_ReplayRecorder", new ReplayRecorder(__instance));
        }
    }
}
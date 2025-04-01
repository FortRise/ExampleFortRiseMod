using System.Xml;
using MonoMod.Utils;
using TowerFall;

namespace MoreReplay;

public static class LevelPatch 
{
    public static void Load() 
    {
        On.TowerFall.Level.ctor += ctor_patch;
    }

    public static void Unload() 
    {
        On.TowerFall.Level.ctor -= ctor_patch;
    }

    private static void ctor_patch(On.TowerFall.Level.orig_ctor orig, TowerFall.Level self, TowerFall.Session session, XmlElement xml)
    {
        orig(self, session, xml);
        var dynSelf = DynamicData.For(self);
        if (session.MatchSettings.SoloMode && SaveData.Instance.Options.ReplayMode != Options.ReplayModes.Off)
        {
            dynSelf.Invoke("set_ReplayRecorder", new ReplayRecorder(self));
        }
    }
}
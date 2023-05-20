using System;
using System.Xml;
using MonoMod.ModInterop;

namespace MapPatchModule;

[ModExportName("MapPatcherHelper")]
public static class ModInterop 
{
    public static void QuestLevelXMLModifier(string levelPath, Action<XmlElement> xmlAction) 
    {
        LevelManager.QuestModifiable.Add(levelPath.Replace('\\', '/'), xmlAction);
    }
}
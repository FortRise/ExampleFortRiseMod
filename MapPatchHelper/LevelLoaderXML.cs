using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using FortRise;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using TowerFall;

namespace MapPatchModule;

public static class LevelManager 
{
    public static Dictionary<string, Action<XmlElement>> QuestModifiable = new();
}

public class LevelLoaderXMLPatch 
{
    private static IDetour hook_Load;
    public static void Load() 
    {
        hook_Load = new ILHook(
            typeof(LevelLoaderXML).GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Instance),
            LevelLoaderXML_LoadPatch
        );
    }

    public static void Unload() 
    {
        hook_Load.Dispose();
    }

    public static void LevelLoaderXML_LoadPatch(ILContext ctx) 
    {
        var cursor = new ILCursor(ctx);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((LevelLoaderXML self) => 
        {
            var levelSystem = self.Session.MatchSettings.LevelSystem;
            if (levelSystem is QuestLevelSystem questSystem) 
            {
                Logger.Log(questSystem.QuestTowerData.Path.Replace('\\', '/'));
                if (LevelManager.QuestModifiable.TryGetValue(questSystem.QuestTowerData.Path.Replace('\\', '/'), out var invoker)) 
                {
                    invoker?.Invoke(self.XML);
                }
            }
        });
    } 

    // public delegate IEnumerator orig_Load(LevelLoaderXML self);

    // public static IEnumerator LevelLoaderXML_LoadPatch(orig_Load orig, LevelLoaderXML self) 
    // {
    //     var levelSystem = self.Session.MatchSettings.LevelSystem;
    //     if (levelSystem is QuestLevelSystem questSystem) 
    //     {
    //         Logger.Log(questSystem.QuestTowerData.Path);
    //         if (LevelManager.QuestModifiable.TryGetValue(questSystem.QuestTowerData.Path, out var invoker)) 
    //         {
    //             invoker?.Invoke(self.XML);
    //         }
    //     }
    //     yield return orig(self);
    // }
}
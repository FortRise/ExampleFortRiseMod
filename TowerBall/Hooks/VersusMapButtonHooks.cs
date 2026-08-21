using System;
using System.IO;
using FortRise;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using TowerFall;

namespace TowerBall;

[HarmonyPatch(typeof(VersusMapButton))]
public static class VersusMapButtonHooks
{
    [HarmonyPatch(MethodType.Constructor, [typeof(VersusTowerData)])]
    [HarmonyPostfix]
    public static void GetLocked_Postfix(MapButton __instance)
    {
        try 
        {
            if (MainMenu.VersusMatchSettings.Mode != TowerBall.TowerBallEntry.Modes)
            {
                return;
            }

            var data = __instance.Data;
            var levelData = data.LevelData;
            if (levelData is VersusTowerData towerData && towerData.Levels.Count > 0)
            {
                var path = Path.GetDirectoryName(towerData.Levels[0].Path)!;
                var towerBall = Path.Combine(path!, "TowerBall");

                if (!path.Contains("mod:") || path.Contains("mod:Teuria.WiderSet"))
                {
                    __instance.Locked = false;
                    return;
                }

                if (!ModIO.IsDirectoryExists(towerBall))
                {
                    __instance.Locked = true;
                    return;
                }

                var files = ModIO.GetFiles(towerBall);

                bool hasOelOrJson = false;
                for (int i = 0; i < files.Length; i += 1)
                {
                    if (files[i].EndsWith(".oel") || files[i].EndsWith(".json"))
                    {
                        hasOelOrJson = true;
                        break;
                    }
                }

                __instance.Locked = hasOelOrJson;

                return;
            }


            __instance.Locked = true;
            return;
        }
        catch (Exception ex)
        {
            TowerBallModModule.Instance.Logger.LogError("An exception occurs: {ex}", ex);
        }
    }
}


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using FortRise;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace TowerBall;

[HarmonyPatch(typeof(TowerMapData))]
public static class TowerMapDataHooks
{
    [HarmonyPatch(nameof(TowerMapData.GetLevelSystem))]
    [HarmonyPrefix]
    public static bool GetLevelSystem(TowerMapData __instance, ref LevelSystem __result)
    {
        try 
        {
            var levelData = Private.Field<TowerMapData, LevelData>("levelData", __instance).Read();
            if (levelData is VersusTowerData data
                && MainMenu.VersusMatchSettings.Mode == TowerBall.TowerBallEntry.Modes)
            {
                __result = new TowerBallVersusLevelSystem(data);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            TowerBallModModule.Instance.Logger.LogError("An exception occurs: {ex}", ex);
            return true;
        }
    }
}

public class TowerBallVersusLevelSystem : VersusLevelSystem
{
    private DynamicData data;

    private string data_lastLevel 
    {
        get => data.Get<string>("lastLevel")!;
        set => data.Set("lastLevel", value);
    }

    private List<string> data_levels
    {
        get => data.Get<List<string>>("levels")!; 
        set => data.Set("levels", value);
    }

    public TowerBallVersusLevelSystem(VersusTowerData tower) : base(tower)
    {
        data = DynamicData.For(this);
    }

    public override XmlElement GetNextRoundLevel(MatchSettings matchSettings, int roundIndex, out int randomSeed)
    {
        if (data_levels.Count == 0)
        {
            GenLevels(matchSettings);
        }

        data_lastLevel = data_levels[0];
        data_levels.RemoveAt(0);
        randomSeed = 0;

        foreach (char c in data_lastLevel)
        {
            randomSeed += c;
        }

        var level = TowerBallModModule.Instance.ModContent.Root.GetRelativePath(data_lastLevel);
        using var fs = level.Stream;

        return Calc.LoadXML(fs)["level"]!;
    }

    private void GenLevels(MatchSettings matchSettings)
    {
        var levels = VersusTowerData.GetLevels(matchSettings);

        var path = Path.GetDirectoryName(levels[0])!;
        var towerBall = Path.Combine(path!, "TowerBall");

        if (!path.Contains("mod:"))
        {
            towerBall = "mod:Suyooo.TowerBall/" + towerBall;
            towerBall = towerBall.Replace("DarkWorldContent", "Content");
        }
        else 
        {
            towerBall = towerBall.Replace("mod:Teuria.WiderSet", "mod:Suyooo.TowerBall");
        }

        // TODO: don't use ResourceTree
        var towerBallDir = RiseCore.ResourceTree.Get(towerBall);
        var files = towerBallDir.Childrens;

        var trueLevels = new List<string>(files.Count);

        foreach (var file in files)
        {
            var p = file.Path;
            if (p.EndsWith(".oel") || p.EndsWith(".json"))
            {
                trueLevels.Add(p);
            }
        }

        data_levels = trueLevels;
    }
}

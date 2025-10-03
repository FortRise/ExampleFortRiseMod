using System;
using System.Collections.Generic;
using FortRise;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class WideTowerManager 
{
    public static WideTowerManager Instance { get; private set; } = new();
    public HashSet<string> VersusLevelSets = [];
    public Dictionary<string, List<VersusLevelData>> MappedLevels = [];

    public WideTowerManager()
    {
        Instance = this;
    }

    public void AddEntry(string levelID, IResourceInfo[] levels)
    {
        var entry = WiderSetModule.Instance.Context.Registry.Towers.GetVersusTower(levelID);
        if (entry is null)
        {
            return;
        }

        VersusLevelSets.Add(entry.LevelSet);

        List<VersusLevelData> mappedLevels = [];

        foreach (var level in levels)
        {
            var xml = level.Xml?["level"];
            if (xml is null)
            {
                continue;
            }

            int playerSpawnCount = 0;
            int teamSpawnsCount = 0;

            var entitiesXml = xml?["Entities"];
            if (entitiesXml is not null)
            {
                playerSpawnCount = entitiesXml.GetElementsByTagName("PlayerSpawn").Count;

                teamSpawnsCount = Math.Min(
                    entitiesXml.GetElementsByTagName("TeamSpawnA").Count,
                    entitiesXml.GetElementsByTagName("TeamSpawnB").Count
                );
            }

            var versusLevel = new VersusLevelData()
            {
                Path = level.RootPath,
                PlayerSpawns = playerSpawnCount,
                TeamSpawns = teamSpawnsCount,
            };

            mappedLevels.Add(versusLevel);
        }

        MappedLevels[entry.ID] = mappedLevels;
    }
}


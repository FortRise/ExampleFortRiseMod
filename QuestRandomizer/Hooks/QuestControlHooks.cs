using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Emit;
using System.Xml;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.QuestRandomizer;

[HarmonyPatch(typeof(QuestControl))]
internal class QuestControlHooks 
{
    private static readonly string[] enemies = [
        "FlamingSkull", 
        "Exploder", 
        "EvilCrystal", 
        "Ghost", 
        "GreenGhost", 
        "Elemental", 
        "GreenElemental", 
        "Slime", 
        "RedSlime", 
        "BlueSlime", 
        "Bat",
        "BombBat", 
        "Crow",
        "Cultist", 
        "ScytheCultist"
    ];

    private static readonly string[] uncommonEnemies = [
        "BlueCrystal", 
        "Mole", 
        "Worm", 
        "SuperBombBat", 
        "BossCultist",
        "Birdman", 
        "DarkBirdman", 
    ];

    private static readonly string[] rareEnemies = [
        "TechnoMage", 
        "PrismCrystal", 
        "BoltCrystal", 

        "Skeleton", 
        "SkeletonS", 
        "BombSkeleton", 
        "BombSkeletonS", 
        "LaserSkeleton", 
        "LaserSkeletonS", 
        "MimicSkeleton", 
        "DrillSkeleton", 
        "DrillSkeletonS", 
        "BoltSkeleton", 
        "BoltSkeletonS", 
        "Jester", 
        "BossSkeleton", 
        "BossWingSkeleton", 
        "WingSkeleton", 
        "WingSkeletonS", 
        "TriggerSkeleton", 
        "TriggerSkeletonS", 
        "PrismSkeleton", 
        "PrismSkeletonS"
    ];

    private static readonly string[] treasures = [
		"Arrows",
		"BombArrows",
		"SuperBombArrows",
		"LaserArrows",
		"BrambleArrows",
		"DrillArrows",
		"BoltArrows",
		"FeatherArrows",
		"TriggerArrows",
		"PrismArrows",
		"Shield",
		"Wings",
		"SpeedBoots",
		"Mirror",
		"Bomb"
    ];

    private static readonly int[] delayTargetSlow = [
        30, 60, 90, 120, 160
    ];

    private static readonly int[] delayTargetFast = [
        30, 60
    ];

    [HarmonyPatch(nameof(QuestControl.Added))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Added_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);
        var noRandomizer = cursor.CreateLabel();
        var randomizer = cursor.CreateLabel();

        cursor.GotoNext(MoveType.After, [ILMatch.Callvirt("LoadSpawns")]);
        cursor.EmitDelegate(() => false);
        cursor.Emit(new CodeInstruction(OpCodes.Brtrue_S, noRandomizer));

        cursor.Emit(new CodeInstruction(OpCodes.Ldarg_0));
        cursor.EmitDelegate((QuestControl __instance) => 
        {
            XmlDocument doc = new XmlDocument();
            var root = doc.CreateElement("data");
            doc.AppendChild(root);

            var normal = doc.CreateElement("normal");
            root.AppendChild(normal);

            var hardcore = doc.CreateElement("hardcore");
            root.AppendChild(hardcore);
            var questData = GenerateData(__instance);

            CreateXml(doc, normal, questData, false);
            CreateXml(doc, hardcore, questData, true);


            doc.Save(Path.Combine(AppContext.BaseDirectory, "DumpLevels", "Debug.xml"));

            return doc;
        });

        cursor.Emit(new CodeInstruction(OpCodes.Stloc, 0));
        cursor.Emit(new CodeInstruction(OpCodes.Br, randomizer));
        cursor.MarkLabel(noRandomizer);

        cursor.GotoNext([ILMatch.Ldarg(0), ILMatch.Ldloc(0), ILMatch.Ldstr()]);
        cursor.GotoPrev();
        cursor.MarkLabel(randomizer);

        cursor.LogInstructions();

        return cursor.Generate();
    }

    private static void CreateXml(XmlDocument doc, XmlElement target, QuestData questData, bool isHardcore)
    {
        foreach (var data in questData.Waves)
        {
            var wave = doc.CreateElement("wave");

            if (data.IsHardcore && !isHardcore)
            {
                continue;
            }

            if (isHardcore)
            {
                if (data.HardcoreFloors.Count > 0)
                {
                    var floors = doc.CreateElement("floors");
                    wave.AppendChild(floors);

                    floors.InnerText = data.HardcoreFloors.Join();
                }
            }
            else 
            {
                if (data.Floors.Count > 0)
                {
                    var floors = doc.CreateElement("floors");
                    wave.AppendChild(floors);

                    floors.InnerText = data.Floors.Join();
                }

            }
            
            if (data.Slow)
            {
                if (data.HardcoreSlow)
                {
                    if (isHardcore)
                    {
                        var slow = doc.CreateAttribute("slow");
                        slow.InnerText = "true";
                        wave.Attributes.Append(slow);
                    }
                }
                else 
                {
                    var slow = doc.CreateAttribute("slow");
                    slow.InnerText = "true";
                    wave.Attributes.Append(slow);
                }
            }

            if (data.Dark)
            {
                if (data.HardcoreDark)
                {
                    if (isHardcore)
                    {
                        var dark = doc.CreateAttribute("dark");
                        dark.InnerText = "true";
                        wave.Attributes.Append(dark);
                    }
                }
                else 
                {
                    var dark = doc.CreateAttribute("dark");
                    dark.InnerText = "true";
                    wave.Attributes.Append(dark);
                }
            }

            foreach (var dgroup in data.Groups)
            {
                var group = doc.CreateElement("group");

                if (dgroup.IsHardcore && !isHardcore)
                {
                    continue;
                }

                if (dgroup.Reaper.TryGetValue(out int value))
                {
                    var reaper = doc.CreateElement("reaper");
                    reaper.InnerText = value.ToString();
                    group.AppendChild(reaper);
                    wave.AppendChild(group);
                    continue;
                }

                if (dgroup.Delay != 0)
                {
                    var attr = doc.CreateAttribute("delay");
                    attr.InnerText = dgroup.Delay.ToString();
                    group.Attributes.Append(attr);
                }

                if (dgroup.Spawns.Count > 0)
                {
                    var actualSpawns = new HashSet<string>();

                    if (isHardcore && dgroup.HardcoreSpawns.Count > 0)
                    {
                        actualSpawns.UnionWith(dgroup.HardcoreSpawns);
                    }
                    actualSpawns.UnionWith(dgroup.Spawns);

                    var spawns = doc.CreateElement("spawns");
                    spawns.InnerText = actualSpawns.Join();
                    group.AppendChild(spawns);
                }

                if (dgroup.Enemies.Count > 0)
                {
                    var actualEnemies = new HashSet<string>();

                    if (isHardcore && dgroup.HardcoreEnemies.Count > 0)
                    {
                        actualEnemies.UnionWith(dgroup.HardcoreEnemies);
                    }
                    actualEnemies.UnionWith(dgroup.Enemies);

                    var enemies = doc.CreateElement("enemies");
                    enemies.InnerText = actualEnemies.Join();
                    group.AppendChild(enemies);
                }

                if (dgroup.Treasures.Count > 0)
                {
                    var treasures = doc.CreateElement("treasure");
                    treasures.InnerText = dgroup.Treasures.Join();
                    group.AppendChild(treasures);
                }

                wave.AppendChild(group);
            }

            target.AppendChild(wave);
        }
    }

    private static QuestData GenerateData(QuestControl __instance)
    {
        var rarityPicker = new WeightedList<string>();

        rarityPicker.Add("common", 1);
        rarityPicker.Add("uncommon", 0.5f);
        rarityPicker.Add("rare", 0.1f);

        var towerData = (__instance.Level.Session.MatchSettings.LevelSystem as QuestLevelSystem)!.QuestTowerData;
        string levelID = towerData.GetLevelID();
        bool canBoss = levelID == "Ascension" || levelID == "KingsCourt";
        var spawns = Private.Field<QuestControl, Dictionary<string, QuestSpawnPortal>>("spawns", __instance)
            .Read();

        var chestSpawns = Private.Field<QuestControl, List<Vector2>>("chestSpawns", __instance)
            .Read();

        var floorMiasmas = Private.Field<QuestControl, List<FloorMiasma>>("floorMiasmas", __instance)
            .Read();

        int maxGroup = floorMiasmas.Count > 0 ? floorMiasmas.Select(x => x.Group).Max() : -1;

        var leftPortal = new List<(string key, QuestSpawnPortal portal)>();
        var middlePortal = new List<(string key, QuestSpawnPortal portal)>();
        var rightPortal = new List<(string key, QuestSpawnPortal portal)>();

        foreach (var (key, value) in spawns)
        {
            if (value.X < 160)
            {
                leftPortal.Add((key, value));
            }

            if (value.X == 160)
            {
                middlePortal.Add((key, value));
            }

            if (value.X > 160)
            {
                rightPortal.Add((key, value));
            }
        }
        var questData = new QuestData();

        Calc.PushRandom(942);
        int minimumWave = 5;
        int minimumGroup = 4;
        int bossCount = 0;
        int treasureCount = chestSpawns.Count;
        
        int hardcoreWaveCount = 3;

        // starting reaper
        if (canBoss)
        {
            bossCount = Calc.Random.InclusiveRange(0, 4);
        }

        bool defeatedAllBosses = false;

        int waveAmount = Calc.Random.InclusiveRange(minimumWave, 10);

        for (int waveIdx = 0; waveIdx < waveAmount; waveIdx += 1)
        {
            var wave = new WaveData();
            questData.Waves.Add(wave);

            bool shouldFloor = maxGroup >= 0 && Calc.Random.InclusiveRange(0, 100) > 90;

            if (shouldFloor)
            {
                bool onlyOne = Calc.Random.InclusiveRange(0, 100) < 80;
                bool isHardcore = Calc.Random.InclusiveRange(0, 100) > 70;

                if (onlyOne)
                {
                    if (isHardcore)
                    {
                        wave.HardcoreFloors.Add(Calc.Random.InclusiveRange(0, maxGroup));
                    }
                    else 
                    {
                        wave.Floors.Add(Calc.Random.InclusiveRange(0, maxGroup));
                    }
                }
                else 
                {
                    int count = Calc.Random.InclusiveRange(0, maxGroup);
                    while (count > 0)
                    {
                        if (isHardcore)
                        {
                            wave.HardcoreFloors.Add(Calc.Random.InclusiveRange(0, maxGroup));
                        }
                        else 
                        {
                            wave.Floors.Add(Calc.Random.InclusiveRange(0, maxGroup));
                        }
                        count -= 1;
                    }
                }
            }
            else 
            {
                bool shouldFloorButHardcore = maxGroup >= 0 && Calc.Random.InclusiveRange(0, 100) > 70;
                if (shouldFloorButHardcore)
                {
                    bool onlyOne = Calc.Random.InclusiveRange(0, 100) < 80;
                    if (onlyOne)
                    {
                        wave.HardcoreFloors.Add(Calc.Random.InclusiveRange(0, maxGroup));
                    }
                    else 
                    {
                        int count = Calc.Random.InclusiveRange(0, maxGroup);
                        while (count > 0)
                        {
                            wave.HardcoreFloors.Add(Calc.Random.InclusiveRange(0, maxGroup));
                            count -= 1;
                        }
                    }
                }
            }

            // should be hardcore?
            if (hardcoreWaveCount > 0 && Calc.Random.InclusiveRange(0, 100) < 20)
            {
                hardcoreWaveCount -= 1;
                wave.IsHardcore = true;
            }

            bool isDark = Calc.Random.InclusiveRange(0, 100) < 20;
            bool isSlow = Calc.Random.InclusiveRange(0, 100) < 5;

            if (isDark)
            {
                bool isHardcore = Calc.Random.InclusiveRange(0, 100) < 80;
                if (isHardcore)
                {
                    wave.HardcoreDark = true;
                }
                wave.Dark = true;
            }

            if (isSlow)
            {
                bool isHardcore = Calc.Random.InclusiveRange(0, 100) < 80;
                if (isHardcore)
                {
                    wave.HardcoreSlow = true;
                }
                wave.Slow = true;
            }

            // if its the last wave and it has boss in it, do the boss no matter what
            if (canBoss && waveIdx == (waveAmount - 1) && !defeatedAllBosses)
            {
                var group = new GroupData
                {
                    Reaper = bossCount
                };
                wave.Groups.Add(group);
                continue;
            }


            if (canBoss)
            {
                bool isReaper = Calc.Random.InclusiveRange(0, 100) > 90;
                if (isReaper)
                {

                    var group = new GroupData
                    {
                        Reaper = bossCount
                    };
                    wave.Groups.Add(group);

                    if (bossCount == 4)
                    {
                        defeatedAllBosses = true;
                    }

                    bossCount = Math.Min(bossCount + 1, 4);
                    continue;
                }
            }

            int groupAmount = Calc.Random.InclusiveRange(minimumGroup, 7);
            int hardcoreGroupCount = 3;

            for (int j = 0; j < groupAmount; j += 1)
            {
                var group = new GroupData();
                wave.Groups.Add(group);

                if (hardcoreGroupCount > 0 && Calc.Random.InclusiveRange(0, 100) < 20)
                {
                    hardcoreGroupCount -= 1;
                    group.IsHardcore = true;
                }

                bool shouldDelay = Calc.Random.InclusiveRange(0, 100) > 50;
                if (shouldDelay)
                {
                    bool shouldBeFast = Calc.Random.InclusiveRange(0, 100) > 70;

                    if (shouldBeFast)
                    {
                        int index = Calc.Random.InclusiveRange(0, delayTargetFast.Length - 1);
                        group.Delay = delayTargetFast[index];
                    }
                    else 
                    {
                        int index = Calc.Random.InclusiveRange(0, delayTargetSlow.Length - 1);
                        group.Delay = delayTargetSlow[index];
                    }
                }


                var left = FindPortal(leftPortal);

                foreach (var (key, portal) in rightPortal)
                {
                    var opposite = WrapMath.Opposite(left.portal.Position);

                    if (portal.Position == opposite)
                    {
                        var e = new HashSet<string>();
                        var s = new HashSet<string>();

                        group.Spawns = s;
                        group.Enemies = e;

                        bool hasTreasures = 
                            treasureCount != 0 && Calc.Random.InclusiveRange(0, 100) > 90;

                        if (hasTreasures)
                        {
                            group.Treasures = GetTreasures(treasureCount);
                            treasureCount -= group.Treasures.Count;
                        }

                        bool beExact = Calc.Random.InclusiveRange(0, 100) > 60;
                        bool caterHardcore = Calc.Random.InclusiveRange(0, 100) > 85;

                        if (IsMirrored())
                        {
                            var already = new HashSet<string>
                            {
                                key
                            };

                            var portals = GetExtraPortalMirrored(already, leftPortal, rightPortal);
                            already.Add(left.key);
                            already.Add(key);

                            bool isOnlyOne = Calc.Random.InclusiveRange(0, 100) > 40;

                            int enemyCount = GetEnemyCount();

                            if (enemyCount % 2 != 0)
                            {
                                enemyCount += 1;
                            }
                            portals.UnionWith(already);
                            s.UnionWith(portals);

                            e.UnionWith(GetEnemySet(
                                        rarityPicker, 
                                        portals.Count, 
                                        enemyCount, 
                                        beExact, 
                                        isOnlyOne));

                            if (caterHardcore)
                            {
                                var hardcorePortals = GetExtraPortalMirrored(already, leftPortal, rightPortal);
                                group.HardcoreSpawns.UnionWith(hardcorePortals);
                                group.HardcoreEnemies.UnionWith(
                                    GetEnemySet(
                                        rarityPicker,
                                        portals.Count, 
                                        Math.Max(enemyCount - 2, 1), 
                                        beExact, 
                                        isOnlyOne));
                            }
                        }
                        else 
                        {
                            var already = new HashSet<string>
                            {
                                key
                            };


                            var portals = GetExtraPortal(already, leftPortal, rightPortal, middlePortal);
                            portals.Add(key);

                            s.UnionWith(portals);

                            int enemyCount = GetEnemyCount();
                            bool isOnlyOne = Calc.Random.InclusiveRange(0, 100) > 50;
                            e.UnionWith(GetEnemySet(
                                    rarityPicker,
                                    portals.Count, 
                                    enemyCount, 
                                    beExact, 
                                    isOnlyOne));

                            if (caterHardcore)
                            {
                                var hardcorePortals = GetExtraPortal(already, leftPortal, rightPortal, middlePortal);
                                group.HardcoreSpawns.UnionWith(hardcorePortals);

                                group.HardcoreEnemies.UnionWith(
                                    GetEnemySet(
                                        rarityPicker,
                                        portals.Count, 
                                        Math.Max(enemyCount - 2, 1), 
                                        beExact, 
                                        isOnlyOne));
                            }
                        }
                    }
                }
            }
        }

        Calc.PopRandom();

        return questData;
    }

    private static HashSet<string> GetTreasures(int count)
    {
        var set = new HashSet<string>();
        int treasureCount = Calc.Random.InclusiveRange(1, 2);
        if (treasureCount > count)
        {
            treasureCount = count;
        }

        for (int i = 0; i < treasureCount; i += 1)
        {
            int index = Calc.Random.InclusiveRange(0, treasures.Length - 1);
            set.Add(treasures[index]);
        }

        return set;
    }

    private static HashSet<string> GetExtraPortal(
        HashSet<string> already,
        List<(string key, QuestSpawnPortal portal)> leftPortal, 
        List<(string key, QuestSpawnPortal portal)> rightPortal,
        List<(string key, QuestSpawnPortal portal)> middlePortal)
    {
        int howMany = Calc.Random.InclusiveRange(0, 4);
        if (howMany == 0)
        {
            return [];
        }
        var result = new HashSet<string>();

        var list = new List<(string key, QuestSpawnPortal portal)>();
        list.AddRange(leftPortal);
        list.AddRange(rightPortal);
        list.AddRange(middlePortal);

        for (int i = 0; i < howMany; i += 1)
        {
            var index = Calc.Random.InclusiveRange(0, list.Count - 1);
            var (key, _) = list[index];
            if (already.Contains(key))
            {
                continue;
            }

            result.Add(key);
        }

        return result;
    }

    private static HashSet<string> GetExtraPortalMirrored(
        HashSet<string> already,
        List<(string key, QuestSpawnPortal portal)> leftPortal, 
        List<(string key, QuestSpawnPortal portal)> rightPortal)
    {
        int howMany = Calc.Random.InclusiveRange(0, 4);
        if (howMany == 0)
        {
            return [];
        }
        var list = new HashSet<string>();

        for (int i = 0; i < howMany; i += 1)
        {
            var lPortal = FindPortal(leftPortal);
            if (already.Contains(lPortal.key))
            {
                continue;
            }

            foreach (var (key, portal) in rightPortal)
            {
                if (lPortal.portal.Position == portal.Position)
                {
                    list.Add(lPortal.key);
                    list.Add(key);
                }
            }
        }

        return list;
    }

    private static int GetEnemyCount()
    {
        return Calc.Random.InclusiveRange(1, 4);
    }

    private static string GetExactEnemy(WeightedList<string> rarityPicker)
    {
        string choice = rarityPicker.GetChoice(Calc.Random);

        int index;
        int length;
        switch (choice)
        {
            case "uncommon":    
                length = uncommonEnemies.Length;
                index = Calc.Random.InclusiveRange(0, length - 1);
                return uncommonEnemies[index];
            case "rare":
                length = rareEnemies.Length;
                index = Calc.Random.InclusiveRange(0, length - 1);
                return rareEnemies[index];
            default:
                length = enemies.Length;
                index = Calc.Random.InclusiveRange(0, length - 1);
                return enemies[index];
        }
    }

    private static HashSet<string> GetEnemySet(
        WeightedList<string> rarityPicker,
        int portalCount, int enemyCount, bool beExact, bool onlyOne)
    {
        if (onlyOne || portalCount == 1)
        {
            // if there's only one, it should be exactly how many portals there are.
            // Otherwise, it just would look awkward
            int limit = portalCount;
            string enemy = GetExactEnemy(rarityPicker);
            if (enemy.Contains("Skeleton"))
            {
                limit = Math.Min(2, limit);
            }

            return [$"[{limit}]{enemy}"];
        }

        if (beExact)
        {
            HashSet<string> enemies = [];
            int count = portalCount;

            while (count > 0)
            {
                string enemy = GetExactEnemy(rarityPicker);
                int limit = count;
                if (enemy.Contains("Skeleton") && limit > 2)
                {
                    limit -= 2;
                }
                int typeCount = Calc.Random.InclusiveRange(1, limit);
                if (typeCount == 1)
                {
                    enemies.Add($"{enemy}");
                }
                else 
                {
                    enemies.Add($"[{typeCount}]{enemy}");
                }
                count -= typeCount;
            }

            return enemies;
        }
        else 
        {
            HashSet<string> enemies = [];
            int count = enemyCount;

            while (count > 0)
            {
                string enemy = GetExactEnemy(rarityPicker);
                int limit = count;
                if (enemy.Contains("Skeleton") && limit > 2)
                {
                    limit -= 2;
                }
                int typeCount = Calc.Random.InclusiveRange(1, limit);
                if (typeCount == 1)
                {
                    enemies.Add($"{enemy}");
                }
                else 
                {
                    enemies.Add($"[{typeCount}]{enemy}");
                }
                count -= typeCount;
            }

            return enemies;
        }
    }

    private static bool IsMirrored()
    {
        return Calc.Random.InclusiveRange(0, 100) > 50;
    }

    private static (string key, QuestSpawnPortal portal) FindPortal(List<(string key, QuestSpawnPortal portal)> portals)
    {
        int x = portals.Count;
        int index = Calc.Random.InclusiveRange(0, x - 1);

        return portals[index];
    }
}

internal class QuestData
{
    public List<WaveData> Waves = [];
}

internal class WaveData 
{
    public bool Dark;
    public bool Slow;

    public bool HardcoreDark;
    public bool HardcoreSlow;
    public bool IsHardcore;
    public HashSet<int> Floors = [];
    public HashSet<int> HardcoreFloors = [];

    public List<GroupData> Groups = [];
}

internal class GroupData 
{
    public Option<int> Reaper;
    public bool IsHardcore;
    public int Delay;
    public HashSet<string> Spawns = [];
    public HashSet<string> Treasures = [];
    public HashSet<string> Enemies = [];

    public HashSet<string> HardcoreSpawns = [];
    public HashSet<string> HardcoreEnemies = [];
}

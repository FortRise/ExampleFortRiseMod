using System;
using System.Reflection.Emit;
using System.Collections.Generic;
using FortRise;
using FortRise.Transpiler;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Discord;
using DisInst = Discord.Discord;
using DisLogLevel = Discord.LogLevel;
using HarmonyLib;
using TowerFall;

namespace DiscordPresence;

public class DiscordPresenceModule : Mod
{
    private static DisInst? DiscordInstance;
    private Activity NextPresence;
    private const string FortRiseIcon = "https://i.imgur.com/jMyu0Hl.png";
    private static bool dirty;
    private static DiscordPresenceModule Instance { get; set; } = null!;

    public DiscordPresenceModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;
        logger.LogInformation("Initializing Discord Presence");

        try 
        {
            DiscordInstance = new DisInst(1139944748220690532L, (ulong)CreateFlags.NoRequireDiscord);
        }
        catch (Exception e)
        {
            logger.LogError("Could not initialize Discord Presence due to an exception: {exception}", e);
            return;
        }

        DiscordInstance.SetLogHook(DisLogLevel.Debug, (level, message) => 
        {
            switch (level)
            {
                case DisLogLevel.Info:
                    logger.LogInformation("[DISCORD]: {message}", message);
                    break;
                case DisLogLevel.Warn:
                    logger.LogWarning("[DISCORD]: {message}", message);
                    break;
                case DisLogLevel.Error:
                    logger.LogError("[DISCORD]: {message}", message);
                    break;
                case DisLogLevel.Debug:
                    logger.LogDebug("[DISCORD]: {message}", message);
                    break;
            }
        });

        context.Harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MainMenu), nameof(MainMenu.Begin)),
            postfix: new HarmonyMethod(MainMenu_Begin_Postfix)
        );

        context.Harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MapScene), nameof(MapScene.Begin)),
            postfix: new HarmonyMethod(MapScene_Begin_Postfix)
        );

        context.Harmony.Patch(
            AccessTools.DeclaredMethod(typeof(TFGame), "Update"),
            postfix: new HarmonyMethod(TFGame_Update_Postfix)
        );

        context.Harmony.Patch(
            AccessTools.DeclaredMethod(typeof(QuestControl), "SpawnWave"),
            new HarmonyMethod(QuestControl_SpawnWave_Prefix)
        );

        context.Harmony.Patch(
            AccessTools.DeclaredMethod(typeof(QuestRoundLogic), nameof(QuestRoundLogic.RegisterEnemyKill)),
            transpiler: new HarmonyMethod(QuestRoundLogic_RegisterEnemyKill_Transpiler)
        );

        OnUnload = Unload;
        context.Events.OnLevelLoaded += OnLevelLoaded;
    }

    private static void ChangePresence(Activity nextActivity) 
    {
        if (DiscordInstance is null)
        {
            return;
        }

        Instance.NextPresence = nextActivity;
        dirty = true;
    }

    private static void Unload(IModuleContext ctx)
    {
        DiscordInstance?.Dispose();
        DiscordInstance = null;
    }


    private static void OnLevelLoaded(object? sender, RoundLogic e)
    {
        if (DiscordInstance is null)
        {
            return;
        }

        var session = e.Session;
        var levelSystem = session.MatchSettings.LevelSystem;
        switch (levelSystem) 
        {
        case DarkWorldLevelSystem dwSystem:
        {
            var levelID = dwSystem.DarkWorldTowerData.GetLevelID() ?? "Official Level";
            var index = levelID.IndexOf('/');
            if (index != -1) 
            {
                levelID = levelID[(index + 1)..];
            }
            var deaths = session.DarkWorldState.Deaths;
            int totalDeaths = 0;
            foreach (var death in deaths) 
            {
                totalDeaths += death;
            }

            var enemyKills = session.DarkWorldState.EnemyKills;
            var totalKills = 0;
            foreach (var enemyKill in enemyKills) 
            {
                totalKills += enemyKill;
            }

            ChangePresence(new() 
            {
                Details = "Playing " + levelID + " | " + "Level: " + (session.RoundIndex + 1),
                State = "Total Deaths: " + totalDeaths + " | " + "Total Kills: " + totalKills,
                Assets = new()
                {
                    SmallImage = GetDarkWorldDifficulty(session.MatchSettings.DarkWorldDifficulty),
                    SmallText = "Dark World",
                    LargeText = "FortRise",
                    LargeImage = FortRiseIcon
                }
            });
        }
            break;
        case VersusLevelSystem versus:
        {
            var levelID = versus.VersusTowerData.GetLevelID() ?? "Official Level";
            var index = levelID.IndexOf('/');
            if (index != -1) 
            {
                levelID = levelID[(index + 1)..];
            }

            ulong totalKills = 0;
            ulong totalDeaths = 0;

            foreach (var stats in session.MatchStats) 
            {
                totalKills += stats.Kills.Kills;
                totalDeaths += stats.Deaths.Kills;
            }
            
            ChangePresence(new() 
            {
                Details = "Playing " + levelID + " | " + "Round: " + (session.RoundIndex + 1),
                State = "Players: " + TFGame.PlayerAmount + " Total Deaths: " + totalDeaths + " | " + "Total Kills: " + totalKills,
                Assets = new() 
                {
                    SmallImage = "versus",
                    SmallText = "Versus",
                    LargeText = "FortRise",
                    LargeImage = FortRiseIcon
                }
            });
        }
            break;
        case QuestLevelSystem questSystem:
        {
            var levelID = questSystem.QuestTowerData.GetLevelID() ?? "Official Level";
            var index = levelID.IndexOf('/');
            if (index != -1) 
            {
                levelID = levelID[(index + 1)..];
            }

            ChangePresence(new() 
            {
                Details = "Playing " + levelID,
                State = "Players: " + TFGame.PlayerAmount,
                Assets = new() 
                {
                    LargeText = "FortRise",
                    LargeImage = FortRiseIcon,
                    SmallImage = GetQuestDifficulty(e.Session.MatchSettings),
                    SmallText = "Quest"
                }
            });
        }
            break;
        case TrialsLevelSystem trialsSystem:
        {
            var trialTowerData = trialsSystem.TrialsLevelData;
            int typeCompletion = 0;
            string bestTime;
            if (trialTowerData.IsOfficialLevelSet()) 
            {
                var data = SaveData.Instance.Trials.Levels[trialTowerData.ID.X][trialTowerData.ID.Y];
                if (data.UnlockedGold)
                    typeCompletion++;
                if (data.UnlockedDiamond)
                    typeCompletion++;
                if (data.UnlockedDevTime)
                    typeCompletion++;
                bestTime = TrialsResults.GetTimeString(TimeSpan.FromTicks(data.BestTime));
            }
            else 
            {
                var data = FortRiseModule.SaveData.AdventureTrials.AddOrGet(trialTowerData.GetLevelID());
                if (data.UnlockedGold)
                    typeCompletion++;
                if (data.UnlockedDiamond)
                    typeCompletion++;
                if (data.UnlockedDevTime)
                    typeCompletion++;
                bestTime = TrialsResults.GetTimeString(TimeSpan.FromTicks(data.BestTime));
            }
            var levelID = trialTowerData.GetLevelID() ?? "Official Level";
            var index = levelID.IndexOf('/');
            if (index != -1) 
            {
                levelID = levelID[(index + 1)..];
            }

            ChangePresence(new() 
            {
                Details = "Playing " + levelID,
                State = "Best Time: " + bestTime,
                Assets = new() 
                {
                    LargeText = "FortRise",
                    LargeImage = FortRiseIcon,
                    SmallImage = GetTrialTime(typeCompletion),
                    SmallText = "Trial"
                }
            });
        }
            break;
        }
    }

    private static string GetTrialTime(int typeCompletion) 
    {
        return typeCompletion switch 
        {
            3 => "trials_pearl",
            2 => "trials_diamond",
            1 => "trials_gold",
            _ => "trials_none"
        };
    }

    private static string GetDarkWorldDifficulty(DarkWorldDifficulties difficulty) 
    {
        return difficulty switch 
        {
            DarkWorldDifficulties.Normal => "normal",
            DarkWorldDifficulties.Hardcore => "hardcore",
            DarkWorldDifficulties.Legendary => "legendary",
            _ => "normal"
        };
    }

    private static void QuestControl_SpawnWave_Prefix(QuestControl __instance, int waveNum)
    {
        if (DiscordInstance is null)
        {
            return;
        }

        var session = __instance.Level.Session;
        if (session.MatchSettings.LevelSystem is not QuestLevelSystem questSystem)
        {
            return;
        }
        var levelID = questSystem.QuestTowerData.GetLevelID() ?? "Official Level";
        var index = levelID.IndexOf('/');
        if (index != -1) 
        {
            levelID = levelID[(index + 1)..];
        }

        ChangePresence(new() 
        {
            Details = "Playing " + levelID + " | " + "Wave: " + (waveNum + 1),
            State = "Players: " + TFGame.PlayerAmount,
            Assets = new() 
            {
                LargeText = "FortRise",
                LargeImage = FortRiseIcon,
                SmallImage = GetQuestDifficulty(session.MatchSettings),
                SmallText = "Quest"
            }
        });
    }

    private static IEnumerable<CodeInstruction> QuestRoundLogic_RegisterEnemyKill_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        Label label = default;

        if (Instance.Context.Flags.IsWindows)
        {
            cursor.GotoNext([ILMatch.Brfalse()]);
        }
        else 
        {
            cursor.GotoNext([ILMatch.Brtrue_S()]);
        }

        label = (Label)cursor.Instruction.operand;

        cursor.GotoNext(MoveType.After, [ILMatch.Callvirt("Decrement")]);
        cursor.MarkLabel(label);

        cursor.Emits([
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldarg_1)
        ]);

        cursor.EmitDelegate((QuestRoundLogic logic, Vector2 vector) => 
        {
            if (DiscordInstance is null)
            {
                return;
            }

            if (logic.GauntletCounter is null)
            {
                return;
            }

            var session = logic.Session;
            if (session.MatchSettings.LevelSystem is not QuestLevelSystem questSystem)
            {
                return;
            }

            var levelID = questSystem.QuestTowerData.GetLevelID() ?? "Official Level";
            var index = levelID.IndexOf('/');
            if (index != -1) 
            {
                levelID = levelID[(index + 1)..];
            }

            ChangePresence(new() 
            {
                Details = "Playing " + levelID,
                State = "Enemies: " + logic.GauntletCounter.Amount + " | " + "Players: " + TFGame.PlayerAmount,
                Assets = new() 
                {
                    LargeText = "FortRise",
                    LargeImage = FortRiseIcon,
                    SmallImage = GetQuestDifficulty(session.MatchSettings),
                    SmallText = "TowerFall"
                }
            });
        });

        return cursor.Generate();
    }

    private static string GetQuestDifficulty(MatchSettings settings) 
    {
        if (settings.QuestHardcoreMode)
        {
            return "hardcore";
        }

        return "normal";
    }

    private static void MainMenu_Begin_Postfix()
    {
        if (DiscordInstance is null)
        {
            return;
        }

        ChangePresence(new() {
            Details = "On Lobby",
            Assets = new() 
            {
                LargeText = "FortRise",
                LargeImage = FortRiseIcon,
                SmallImage = "versus",
                SmallText = "TowerFall"
            }
        });
    }

    private static void MapScene_Begin_Postfix(MapScene __instance) 
    {
        if (DiscordInstance is null)
        {
            return;
        }

        ChangePresence(new Activity {
            Details = "Selecting a Tower",
            Assets = new() {
                LargeText = "FortRise",
                LargeImage = FortRiseIcon,
                SmallImage = GetMap(__instance.Mode),
                SmallText = __instance.Mode.ToString()
            }
        });

        static string GetMap(MainMenu.RollcallModes mode) 
        {
            return mode switch 
            {
                MainMenu.RollcallModes.DarkWorld => "darkworldmap",
                MainMenu.RollcallModes.Quest => "questmap",
                MainMenu.RollcallModes.Versus => "versus",
                MainMenu.RollcallModes.Trials => "trials",
                _ => "hardcore"
            };
        }
    }

    private static void TFGame_Update_Postfix() 
    {
        if (DiscordInstance is null)
        {
            return;
        }

        if (dirty) 
        {
            DiscordInstance.GetActivityManager().UpdateActivity(Instance.NextPresence, (result) => {
                if (result == Result.Ok) 
                {
                    Instance.Logger.LogDebug("Presence changed successfully!");
                }
                else 
                {
                    Instance.Logger.LogWarning("Failed to change presence: {result}", result);
                }
            });
            dirty = false;
        }

        try 
        {
            DiscordInstance.RunCallbacks();
        } 
        catch (ResultException e) 
        {
            if (e.Message == nameof(Result.NotRunning)) 
            {
                Instance.Logger.LogWarning("Discord was shut down! Disposing Game SDK.");
                Unload(Instance.Context);
                return;
            }
            throw;
        }
    }
}

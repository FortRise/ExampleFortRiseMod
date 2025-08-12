using System;
using FortRise;
using Microsoft.Extensions.Logging;
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
            logger.LogError("Could not initialize Discord Presence");
            logger.LogError(e.ToString());
            return;
        }

        DiscordInstance.SetLogHook(DisLogLevel.Debug, (level, message) => 
        {
            switch (level)
            {
                case DisLogLevel.Info:
                    logger.LogInformation(message);
                    break;
                case DisLogLevel.Warn:
                    logger.LogWarning(message);
                    break;
                case DisLogLevel.Error:
                    logger.LogError(message);
                    break;
                case DisLogLevel.Debug:
                    logger.LogDebug(message);
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

        OnUnload = Unload;
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
        ChangePresence(new Discord.Activity {
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
                if (result == Discord.Result.Ok) 
                {
                    DiscordPresenceModule.Instance.Logger.LogDebug("Presence changed successfully!");
                }
                else 
                {
                    DiscordPresenceModule.Instance.Logger.LogWarning($"Failed to change presence: {result}");
                }
            });
            dirty = false;
        }

        try 
        {
            DiscordInstance.RunCallbacks();
        } 
        catch (Discord.ResultException e) 
        {
            if (e.Message == nameof(Discord.Result.NotRunning)) 
            {
                DiscordPresenceModule.Instance.Logger.LogWarning("[DISCORD] Discord was shut down! Disposing Game SDK.");
                Unload(DiscordPresenceModule.Instance.Context);
                return;
            }
            throw;
        }
    }
}

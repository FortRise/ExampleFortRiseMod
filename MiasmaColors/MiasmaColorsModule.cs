using System;
using System.Collections.Generic;
using System.Reflection;
using FortRise;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace MiasmaColors;

public class MiasmaColorsModule : Mod
{
    public static MiasmaColorsModule Instance = null!;
    
    private static FieldInfo colorField;
    private static FieldInfo lightField;

    static MiasmaColorsModule()
    {
        colorField = typeof(Miasma).GetField("Color", BindingFlags.NonPublic | BindingFlags.Static);
        lightField = typeof(Miasma).GetField("Light", BindingFlags.NonPublic | BindingFlags.Static);
    }

    public MiasmaColorsModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;

        OnLoad = HandleGameLoad;
    }

    private void HandleGameLoad(IModuleContext context)
    {
        context.Events.OnLevelLoaded += HandleLevelLoaded;
        context.Harmony.Patch(AccessTools.Method(typeof(LevelSystem), nameof(LevelSystem.Dispose)), postfix: new HarmonyMethod(DisposePostfix));
    }

    private void HandleLevelLoaded(object sender, RoundLogic logic)
    {
        if (logic.CanMiasma)
        {
            var levelTag = ReadLevelTag(logic.Session.CurrentLevel.SceneTags);
            if (string.IsNullOrEmpty(levelTag))
                return;

            float shiftAmount = GetHueShiftForLevel(levelTag);

            Subtexture miasmaTexture = TFGame.Atlas["miasma/surface"];
            Color[] surfaceColors = new Color[miasmaTexture.Width * miasmaTexture.Height];
            miasmaTexture.Texture2D.GetData(0, miasmaTexture.Rect, surfaceColors, 0, surfaceColors.Length);
            for (int i = 0; i < surfaceColors.Length; i++) 
            {
                if (surfaceColors[i].A == 0)
                    continue;

                HSV hsvColor = ColorUtils.ColorToHSV(surfaceColors[i]);
                ColorUtils.HueShift(ref hsvColor, shiftAmount);
                surfaceColors[i] = ColorUtils.HSVToColor(hsvColor, surfaceColors[i].A);
            }
            miasmaTexture.Texture2D.SetData(0, miasmaTexture.Rect, surfaceColors, 0, surfaceColors.Length);
        
            colorField.SetValue(null,surfaceColors[0]);
            // lightField.SetValue(null, color.Invert());
        }
    }

    private static void DisposePostfix(LevelSystem __instance)
    {
       Instance.Logger.Log(LogLevel.Warning, "Disssposseeee");
    }

    private float GetHueShiftForLevel(string levelTag)
    {
        return levelTag switch 
        {
            "SacredGround" => 132.8f,
            "TwilightSpire" => 27.4f,
            "Backfire" => -81.2f,
            "Flight" => 113f,
            "Mirage" => 143.8f,
            "Thornwood" => -128.4f,
            "FrostfangKeep" => -96.6f,
            "KingsCourt" => 80f,
            "SunkenCity" => -125f,
            "Moonstone" => -62f,
            "TowerForge" => -92f,
            "Ascension" => -95.5f,
            "GauntletA" => -63.7f,
            "GauntletB" => -95.5f,
            _ => 0
        };
    }
    
    private static string ReadLevelTag(List<string> tag) 
    {
        for (int i = 0; i < tag.Count; i++) 
        {
            var spanTag = tag[i].AsSpan();
            if (!spanTag.StartsWith("level".AsSpan(), StringComparison.InvariantCulture)) 
                continue;
            
            return spanTag.Slice(6).ToString();
        }
        return string.Empty;
    }
}
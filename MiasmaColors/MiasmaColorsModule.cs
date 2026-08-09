using System;
using System.Collections.Generic;
using System.Reflection;
using FortRise;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using TowerFall;

namespace MiasmaColors;

public class MiasmaColorsModule : Mod
{
    public static MiasmaColorsModule Instance = null!;

    private DynamicData miasmaData;
    private DynamicData bottomMiasmaData;
    private Color originalColor;
    private Color originalLightColor;
    
    private SubTextureModifier surfaceModifier;
    private SubTextureModifier tentaclesModifier;
    private SubTextureModifier tentaclesHModifier;
    private SubTextureModifier floorModfier;
    
    private bool colorsUpdated = false;

    public MiasmaColorsModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;
        
        miasmaData = new DynamicData(typeof(Miasma));
        bottomMiasmaData = new DynamicData(typeof(BottomMiasma));

        originalColor = miasmaData.Get<Color>("Color");
        originalLightColor = miasmaData.Get<Color>("Light");
        
        OnInitialize = HandleInitialize;
    }

    private void HandleInitialize(IModuleContext context)
    {
        floorModfier = new SubTextureModifier("quest/floorMiasma");
        surfaceModifier = new SubTextureModifier("miasma/surface");
        tentaclesModifier = new SubTextureModifier("miasma/tentacles");
        tentaclesHModifier = new SubTextureModifier("miasma/tentaclesH");
        
        context.Events.OnLevelLoaded += HandleLevelLoaded;
        context.Harmony.Patch(AccessTools.Method(typeof(LevelSystem), nameof(LevelSystem.Dispose)), postfix: new HarmonyMethod(LevelSystemDisposePostfix));
    }

    private void HandleLevelLoaded(object sender, RoundLogic logic)
    {
        if (!logic.CanMiasma || colorsUpdated)
            return;
        
        var levelTag = ReadLevelTag(logic.Session.CurrentLevel.SceneTags);
        if (string.IsNullOrEmpty(levelTag))
            return;

        float shiftAmount = GetHueShiftForLevel(levelTag);
        
        surfaceModifier.ApplyHueShift(shiftAmount);
        tentaclesModifier.ApplyHueShift(shiftAmount);
        floorModfier.ApplyHueShift(shiftAmount);

        Color newColor = ColorUtils.HueShift(originalColor, shiftAmount);
        miasmaData.Set("Color", newColor);
        bottomMiasmaData.Set("Color", newColor);
        
        Color newLightColor = ColorUtils.HueShift(originalLightColor, shiftAmount);
        miasmaData.Set("Light", newLightColor);
        bottomMiasmaData.Set("Light", newLightColor);
    }

    private static void LevelSystemDisposePostfix(LevelSystem __instance)
    {
        if (!Instance.colorsUpdated)
            return;
        
        Instance.surfaceModifier.Reset();
        Instance.floorModfier.Reset();
        Instance.tentaclesModifier.Reset();
        Instance.tentaclesHModifier.Reset();
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
            "TowerForge" => 69.1f,
            "Ascension" => -95.5f,
            "GauntletA" => -63.7f,
            "GauntletB" => -95.5f,
            "TheAmaranth" => -152.6f,
            "Dreadwood" => -180f,
            "Darkfang" => -92.2f,
            "Cataclysm" => 40.6f, 
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
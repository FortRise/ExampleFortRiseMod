using System;
using System.Collections.Generic;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace MiasmaColors;

[Fort("com.terria.miasmacolors", "MiasmaColors")]
public class MiasmaColorsModule : FortModule
{
    public override void Load()
    {
        On.TowerFall.FloorMiasma.ctor += ApplyLightColorToMiasma;
    }

    public override void Unload()
    {
        On.TowerFall.FloorMiasma.ctor -= ApplyLightColorToMiasma;
    }

    private void ApplyLightColorToMiasma(On.TowerFall.FloorMiasma.orig_ctor orig, FloorMiasma self, Vector2 position, int width, int group)
    {
        orig(self, position, width, group);
        self.Add(new MiasmaColorComponent());
    }
}

public class MiasmaColorComponent : Component
{
    public MiasmaColorComponent() : base(false, false)
    {
    }

    public override void EntityAdded()
    {
        base.EntityAdded();

        var levelTag = ReadLevelTag(Scene.SceneTags);
        if (string.IsNullOrEmpty(levelTag))
            return;

        (Entity as LevelEntity).LightColor = (levelTag switch 
        {
            "Sacred Ground" => Calc.HexToColor("ffd29e"),
            "Twilight Spire" => Calc.HexToColor("e39eff"),
            "Backfire" => Calc.HexToColor("9ecdff"),
            "Flight" => Calc.HexToColor("ff9620"),
            "Mirage" => Calc.HexToColor("ffff60"),
            "Thornwood" => Calc.HexToColor("72ff66"),
            "Frostfang Keep" => Calc.HexToColor("26fff5"),
            "King's Court" => Calc.HexToColor("ff0000"),
            "Sunken City" => Calc.HexToColor("50ffb1"),
            "Moonstone" => Calc.HexToColor("7275dd"),
            "Towerforge" => Calc.HexToColor("FF2000"),
            "Ascension" => Calc.HexToColor("f8ffff"),
            "Gauntlet" => Calc.HexToColor("616a9e"),
            "Gauntlet II" => Calc.HexToColor("fbfcfc"),
            _ => Calc.HexToColor("9ED1FF")
        }).Invert();
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
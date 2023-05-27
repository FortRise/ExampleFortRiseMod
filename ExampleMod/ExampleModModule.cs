using System.Xml;
using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.ModInterop;
using TowerFall;

namespace ExampleMod;


[Fort("com.terriatf.testmod", "ExampleMod")]
public class ExampleModModule : FortModule
{
    public static Atlas ExampleAtlas;
    public static SpriteData Data;

    public static ExampleModModule Instance;
    private Harmony harmony;

    public ExampleModModule() 
    {
        Instance = this;
    }
    public override Type SettingsType => typeof(ExampleModSettings);
    public static ExampleModSettings Settings => (ExampleModSettings)Instance.InternalSettings;

    public override void LoadContent()
    {
        ExampleAtlas = Content.LoadAtlas("Atlas/pinkSlime.xml", "Atlas/pinkSlime.png", true);
        Data = Content.LoadSpriteData("Atlas/spriteData.xml", ExampleAtlas);
    }

    public override void Load()
    {
        Settings.FlightTest = () => 
        {
            Music.Play("Flight");
        };
        harmony = new Harmony("com.terriatf.ExampleMod");
        // Uncomment this line to patch all of Harmony's patches
        harmony.PatchAll();

        // PinkSlime.LoadPatch();
        // TriggerBrambleArrow.Load();
        // PatchEnemyBramble.Load();
        BrambleFunPatcher.Load();

        typeof(ModExports).ModInterop();
    }

    public override void Initialize()
    {
        ModExports.QuestLevelXMLModifier?.Invoke("Content/Levels/Quest/00.oel", x => {
            var playerSpawns = x["Entities"].GetElementsByTagName("PlayerSpawn");
            playerSpawns[0].Attributes["x"].Value = "50";
            playerSpawns[1].Attributes["x"].Value = "250";
        });
        var vector2 = new Vector2(40, 40);
        Logger.Log(vector2.X);
        Logger.Log(vector2.Y);
    }

    public override void Unload()
    {
        // PinkSlime.UnloadPatch();
        // TriggerBrambleArrow.Unload();
        // PatchEnemyBramble.Unload();
        BrambleFunPatcher.Unload();
        harmony.UnpatchAll("com.terriatf.ExampleMod");
    }
}

// Harmony can be supported

[HarmonyPatch]
public class MyPatcher 
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenu), "BoolToString")]
    static void BoolToStringPostfix(ref string __result) 
    {
        if (__result == "ON") 
        {
            __result = "ENABLED";
            return;
        }
        __result = "DISABLED";
    }
}


/* 
Example of interppting with libraries
Learn more: https://github.com/MonoMod/MonoMod/blob/master/README-ModInterop.md
*/

[ModImportName("MapPatcherHelper")]
public static class ModExports 
{
    public static Action<string, Action<XmlElement>> QuestLevelXMLModifier;
}
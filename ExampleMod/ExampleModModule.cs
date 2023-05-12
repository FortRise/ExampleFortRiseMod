using FortRise;
using HarmonyLib;
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

    public ExampleModModule() 
    {
        Instance = this;
    }
    public override Type SettingsType => typeof(ExampleModSettings);
    public static ExampleModSettings Settings => (ExampleModSettings)Instance.InternalSettings;

    public override void LoadContent()
    {
        ExampleAtlas = Atlas.Create("Atlas/pinkSlime.xml", "Atlas/pinkSlime.png", true, ContentAccess.ModContent);
        Data = SpriteData.Create("Atlas/spriteData.xml", ExampleAtlas, ContentAccess.ModContent);
    }

    public override void Load()
    {
        Settings.FlightTest = () => 
        {
            Music.Play("Flight");
        };
        var harmony = new Harmony("com.terriatf.ExampleMod");
        // Uncomment this line to patch all of Harmony's patches
        // harmony.PatchAll();

        PinkSlime.LoadPatch();

        typeof(ModExports).ModInterop();
    }

    public override void Unload()
    {
        PinkSlime.UnloadPatch();
    }
}

// Harmony can be supported

[HarmonyPatch(typeof(MainMenu), "BoolToString")]
public class MyPatcher 
{
    static void Postfix(ref string __result) 
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

[ModExportName("ExampleModExport")]
public static class ModExports 
{
    public static int Add(int x, int y) => x + y;
}
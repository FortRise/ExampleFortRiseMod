using System.Xml;
using FortRise;
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

    public ExampleModModule() 
    {
        Instance = this;
    }
    public override Type SettingsType => typeof(ExampleModSettings);
    public static ExampleModSettings Settings => (ExampleModSettings)Instance.InternalSettings;

    public override Type SaveDataType => typeof(ExampleModSaveData);
    public static ExampleModSaveData SaveData => (ExampleModSaveData)Instance.InternalSaveData; 

    public override void LoadContent()
    {
        ExampleAtlas = Content.LoadAtlas("Atlas/pinkSlime.xml", "Atlas/pinkSlime.png");
        Data = Content.LoadSpriteData("Atlas/spriteData.xml", ExampleAtlas);
    }

    public override void Load()
    {
        Settings.FlightTest = () => 
        {
            Music.Play("Flight");
        };

        PinkSlime.LoadPatch();
        TriggerBrambleArrow.Load();
        PatchEnemyBramble.Load();
        BrambleFunPatcher.Load();

        typeof(MapPatchImport).ModInterop();
    }

    public override void Initialize()
    {
        MapPatchImport.QuestLevelXMLModifier?.Invoke("Content/Levels/Quest/00.oel", x => {
            var playerSpawns = x["Entities"].GetElementsByTagName("PlayerSpawn");
            playerSpawns[0].Attributes["x"].Value = "50";
            playerSpawns[1].Attributes["x"].Value = "250";
        });
    }

    public static bool Hey;

    public override void CreateModSettings(TextContainer optionList)
    {
        var toggler = new TextContainer.Toggleable("CONFIRM ME", Hey);
        toggler.Change(x => {
            Hey = x;
        });
        optionList.Add(toggler);
    }

    public override void Unload()
    {
        PinkSlime.UnloadPatch();
        TriggerBrambleArrow.Unload();
        PatchEnemyBramble.Unload();
        BrambleFunPatcher.Unload();
    }
}


/* 
Example of interppting with libraries
Learn more: https://github.com/MonoMod/MonoMod/blob/master/README-ModInterop.md
*/

[ModImportName("MapPatcherHelper")]
public static class MapPatchImport 
{
    public static Action<string, Action<XmlElement>> QuestLevelXMLModifier;
}
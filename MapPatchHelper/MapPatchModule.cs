using FortRise;
using Monocle;
using MonoMod.ModInterop;
using TowerFall;

namespace MapPatchModule;


[Fort("com.terria.MapPatchModule", "MapPatchHelper")]
public class MapPatchHelperModule : FortModule
{
    public static MapPatchHelperModule Instance;

    public MapPatchHelperModule() 
    {
        Instance = this;
    }

    public override void LoadContent()
    {
    }

    public override void Load()
    {
        LevelLoaderXMLPatch.Load();
        typeof(ModInterop).ModInterop();
    }

    public override void Unload()
    {
        LevelLoaderXMLPatch.Unload();
    }
}

using System;
using FortRise;

namespace MusicRestart;

[Fort("com.terria.musicrestart", "Music Restart")]
public sealed class MusicRestartModule : FortModule
{
    public static MusicRestartModule Instance;

    public override Type SettingsType => typeof(MusicRestartSettings);
    public static MusicRestartSettings Settings => (MusicRestartSettings)Instance.InternalSettings;


    public MusicRestartModule() 
    {
        Instance = this;
    }

    public override void Load()
    {
        PauseMenuPatch.Load();
    }

    public override void Unload()
    {
        PauseMenuPatch.Unload();
    }
}

public sealed class MusicRestartSettings : ModuleSettings 
{
    public bool Active = true;
}
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FortRise;
using Microsoft.Extensions.Logging;
using Teuria.WiderSet;

namespace Teuria.Profiles;

public sealed class ProfilesModule : Mod
{
    public static ProfilesModule Instance { get; private set; } = null!;
    public bool[] RollcallProfileActive;
    public IMenuStateEntry ManageProfileState;
    public IMenuStateEntry SelectArcherState;
    public IMenuStateEntry GamepadProfileState;
    public IMenuStateEntry KeyboardProfileState;
    public List<PlayerProfile> Profiles { get; private set; }

    public BundleStateManager bundleStateManager;


    public ProfilesModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        bundleStateManager = new();
        Profiles = [];
        Instance = this;

        var widerSetApi = context.Interop.GetApi<IWiderSetModApi>("Teuria.WiderSet");
        if (widerSetApi is not null)
        {
            RollcallProfileActive = new bool[8];
        }
        else
        {
            RollcallProfileActive = new bool[4];
        }

        ManageProfileState = context.Registry.MenuStates.RegisterMenuState("ManageProfile", new()
        {
            MenuStateType = typeof(ManageProfileMenuState)
        });

        SelectArcherState = context.Registry.MenuStates.RegisterMenuState("SelectArcher", new()
        {
            MenuStateType = typeof(SelectArcherState)
        });

        GamepadProfileState = context.Registry.MenuStates.RegisterMenuState("ManageGamepadProfile", new()
        {
            MenuStateType = typeof(ManageGamepadProfileState)
        });

        KeyboardProfileState = context.Registry.MenuStates.RegisterMenuState("ManageKeyboardProfile", new()
        {
            MenuStateType = typeof(ManageKeyboardProfileState)
        });


        context.Harmony.PatchAll(typeof(ProfilesModule).Assembly);

        context.Events.OnBeforeDataLoad += BeforeDataLoad;
        context.Events.OnBeforeSaveSaveData += BeforeSaveData;
    }

    private void BeforeDataLoad(object? sender, DataLoadEventArgs e)
    {
        var profilesDir = Context.Storage.Open("Profiles");
        if (profilesDir is null)
        {
            return;
        }

        foreach (var child in profilesDir.Childrens)
        {
            var ext = Path.GetExtension(child.Path);
            if (ext != ".json")
            {
                continue;
            }

            var stream = child.ReadStream;
            Profiles.Add(JsonSerializer.Deserialize<PlayerProfile>(stream)!);
        }
    }

    private void BeforeSaveData(object? sender, BeforeSaveSaveDataEventArgs e)
    {
        var profilesDir = Context.Storage.CreateDirectory("Profiles");

        foreach (var profile in Profiles)
        {
            var profileName = profile.Name + ".json";
            var file = profilesDir.AddFile(profileName);

            var json = JsonSerializer.Serialize(profile);
            Context.Storage.WriteAllText(file, json);
        }
        
    }

    public override ModuleSettings? CreateSettings()
    {
        return new ProfileSettings();
    }
}

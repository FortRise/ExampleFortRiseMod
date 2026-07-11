using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FortRise;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Monocle;
using Teuria.WiderSet;
using TowerFall;

namespace Teuria.Profiles;

public sealed class ProfilesModule : Mod
{
    public static ProfilesModule Instance { get; private set; } = null!;
    public PlayerProfile?[] ProfileActive;
    public HashSet<string> NamesActive = [];
    public bool[] RollcallProfileActive;
    public IMenuStateEntry ManageProfileState;
    public IMenuStateEntry SelectArcherState;
    public IMenuStateEntry GamepadProfileState;
    public IMenuStateEntry KeyboardProfileState;
    public List<PlayerProfile> Profiles { get; private set; }
    public Subtexture SingleLock = null!;

    public IWiderSetModApi? IWiderSetModAPI;

    public BundleStateManager bundleStateManager;

    public List<PlayerProfile> EnabledProfile
    {
        get
        {
            var list = new List<PlayerProfile>();
            foreach (var prof in Profiles)
            {
                if (prof.Disabled)
                {
                    continue;
                }

                list.Add(prof);
            }

            return list;
        }
    }


    public ProfilesModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        bundleStateManager = new();
        Profiles = [];
        Instance = this;

        IWiderSetModAPI = context.Interop.GetApi<IWiderSetModApi>("Teuria.WiderSet");
        int playerCount = IWiderSetModAPI is not null ? 8 : 4;

        RollcallProfileActive = new bool[playerCount];
        ProfileActive = new PlayerProfile[playerCount];

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
        context.Events.OnModInitialize += OnModInitialize;
    }

    public void ActivateProfile(int playerIndex, PlayerProfile profile)
    {
        var previousActiveProfile = ProfileActive[playerIndex];
        if (previousActiveProfile is not null)
        {
            NamesActive.Remove(previousActiveProfile.Name);
        }

        ProfileActive[playerIndex] = profile;
        NamesActive.Add(profile.Name);
    }

    public void DeactivateProfile(int playerIndex)
    {
        var profile = ProfileActive[playerIndex];
        if (profile is null)
        {
            return;
        }

        ProfileActive[playerIndex] = null;
        NamesActive.Remove(profile.Name);
    }

    private void OnModInitialize(object? sender, ModuleMetadata e)
    {
        var singleLock = TFGame.MenuAtlas["portraits/archerLock"];
        SingleLock = new Subtexture(singleLock.Texture, new Rectangle(singleLock.X, singleLock.Y, 60, 60));
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

    public override ModuleSaveData? CreateSaveData()
    {
        return new ProfileSaveData();
    }
}

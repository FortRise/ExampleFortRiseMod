using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Xml;
using FortRise;
using FortRise.Content;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.WiderSet;

public class UnsafePlayer
{
    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "wasColliders")]
    public static extern ref Collider[] GetPlayerWasColliders(Player player);
}

public class UnsafeLogo
{
    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "titleTweenFrom")]
    public static extern ref Vector2 GetTitleTweenFrom(Logo? logo);

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "arrowTweenFrom")]
    public static extern ref Vector2 GetArrowTweenFrom(Logo? logo);
}

public class WiderSetModule : Mod
{
    private static readonly Type[] Hookables = [
        typeof(EditorBaseHooks),

        typeof(ActorHooks),
        typeof(ArcherPortraitHooks),
        typeof(BackgroundHooks),
        typeof(EnemyHooks),
        typeof(FightButtonHooks),
        typeof(GameplayLayerHooks),
        typeof(GifExporterHooks),
        typeof(GifEncoderHooks),
        typeof(HUDFadeHooks),
        typeof(IntroSceneHooks),

        typeof(LavaHooks),
        typeof(LayerHooks),
        typeof(LevelHooks),
        typeof(LevelEntityHooks),
        typeof(LevelLoaderXMLHooks),
        typeof(LevelRandomBGDetailsHooks),
        typeof(LevelRandomBGTilesHooks),
        typeof(LevelRandomGeometryHooks),
        typeof(LevelRandomItemsHooks),
        typeof(LevelRandomTreasureHooks),
        typeof(LevelSystemHooks),
        typeof(LevelTilesHooks),
        typeof(LevelBGTilesHooks),
        typeof(LightingLayerHooks),
        typeof(LoopPlatformHooks),
        typeof(MainMenuHooks),
        typeof(MapSceneHooks),
        typeof(MatchTeamsHooks),
        typeof(MatchSettingsHooks),
        typeof(MenuBackgroundHooks),
        typeof(MenuButtonGuideHooks),

        typeof(MiasmaHooks),
        typeof(MInputHooks),
        typeof(OrbLogicHooks),
        typeof(PlayerInputHooks),
        typeof(PauseMenuHooks),
        typeof(ReplayFrameHooks),
        typeof(ReplayViewerHooks),
        typeof(RollcallElementHooks),
        typeof(RoundLogicHooks),
        typeof(TFGameHooks),
        typeof(TreasureSpawnerHooks),
        typeof(SavingInfoSceneHooks),
        typeof(ScreenHooks),
        typeof(SessionHooks),
        typeof(SwitchBlockHooks),
        typeof(VariantHooks),

        typeof(VariantPerPlayerHooks),
        typeof(VersusAwardsHooks),
        typeof(VersusLevelSystemHooks),
        typeof(VersusStartHooks),
        typeof(VersusMatchResultsHooks),
        typeof(VersusPlayerMatchResultsHooks),
        typeof(VersusRoundResultsHooks),
        typeof(VersusTowerDataHooks),
        typeof(WrapHitboxHooks),
        typeof(WrapMathHooks)
    ];


    public static bool IsWide
    {
        get
        {
            return wide;
        }
        set
        {
            DirtyWide = true;
            wide = value;
            if (value)
            {
                WrapMath.AddWidth = new Vector2(420, 0);
            }
            else 
            {
                WrapMath.AddWidth = new Vector2(320, 0);
            }
        }
    }
    public static bool DirtyWide { get; internal set; }
    public static bool AboutToGetWide { get; internal set; }
    public static bool InsideOfTheEditor { get; internal set; }
    private static bool wide;
    public static IMenuSpriteContainerEntry StandardSetSprite { get; internal set; } = null!;
    public static IMenuSpriteContainerEntry WideSetSprite { get; internal set; } = null!;
    public static IMenuStateEntry StandardSelectionEntry { get; internal set; } = null!;

    public static Dictionary<int, Vector2> NotJoinedCharacterOffset = [];
    public static Dictionary<int, Vector2> NotJoinedSecretCharacterOffset = [];
    public static Dictionary<int, Vector2> NotJoinedAltCharacterOffset = [];

    public static Dictionary<int, Vector2> JoinedCharacterOffset = [];
    public static Dictionary<int, Vector2> JoinedSecretCharacterOffset = [];
    public static Dictionary<int, Vector2> JoinedAltCharacterOffset = [];

    public static Dictionary<string, XmlElement> WideBG = [];
    public static Dictionary<string, string> WideRedirector = [];
    public static Dictionary<string, IVersusTowerEntry> MapEntry = [];

    public static ISubtextureEntry SilverPortrait { get; private set; } = null!; 
    public static ISubtextureEntry GoldPortrait { get; private set; } = null!;

    public static WiderSetModule Instance { get; private set; } = null!;

    public WiderSetModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        IsWide = false;
        Instance = this;
        // patch all arrays that only has limited size
        TFGame.Players = new bool[8];
        TFGame.Characters = new int[8];
        TFGame.AltSelect = new ArcherData.ArcherTypes[8];
        TFGame.CoOpCrowns = new bool[8];
        Session.TeamStartArrows = [3, 3, 3, 3, 2, 2, 2];

        for (int i = 4; i < 8; i++)
        {
            TFGame.Characters[i] = i;
        }

        var chestChances = TreasureSpawner.ChestChances;
        Array.Resize(ref chestChances, chestChances.Length + 4);
        chestChances[3] = [0.9f, 0.9f, 0.8f, 0.8f, 0.2f, 0.1f];
        chestChances[4] = [0.9f, 0.9f, 0.9f, 0.8f, 0.4f, 0.1f];
        chestChances[5] = [0.9f, 0.9f, 0.9f, 0.8f, 0.4f, 0.2f];
        chestChances[6] = [0.9f, 0.9f, 0.9f, 0.9f, 0.4f, 0.2f, 0.1f];
        TreasureSpawner.ChestChances = chestChances;

        // logo
        ref var arrowTweenFrom = ref UnsafeLogo.GetArrowTweenFrom(null);
        ref var titleTweenFrom = ref UnsafeLogo.GetTitleTweenFrom(null);

        arrowTweenFrom += new Vector2(100, 0);
        titleTweenFrom -= new Vector2(100, 0);

        SDL3.SDL.SDL_SetHint(SDL3.SDL.SDL_HINT_JOYSTICK_RAWINPUT, "1");
        Environment.SetEnvironmentVariable("FNA_GAMEPAD_NUM_GAMEPADS", "8");

        ref var colliders = ref UnsafePlayer.GetPlayerWasColliders(null!);
        Array.Resize(ref colliders, 8);

        StandardSetSprite = context.Registry.Sprites.RegisterMenuSprite<int>(
            "StandardSet",
            new()
            {
                Texture = context.Registry.Subtextures.RegisterTexture(
                    content.Root.GetRelativePath("Content/images/sets/standardMode.png"),
                    SubtextureAtlasDestination.MenuAtlas
                ),
                FrameWidth = 96,
                FrameHeight = 96,
                OriginX = 48,
                OriginY = 48,
                Animations = [
                    new() { ID = 0, Delay = 0.06f, Loop = true, Frames = [12]},
                    new() { ID = 1, Delay = 0.06f, Loop = true, Frames = [0,1,2,3,4,5,6,7,8,9,10,11]},
                ]
            }
        );

        WideSetSprite = context.Registry.Sprites.RegisterMenuSprite<int>(
            "WideSet",
            new()
            {
                Texture = context.Registry.Subtextures.RegisterTexture(
                    content.Root.GetRelativePath("Content/images/sets/wideMode.png"),
                    SubtextureAtlasDestination.MenuAtlas
                ),
                FrameWidth = 96,
                FrameHeight = 96,
                OriginX = 48,
                OriginY = 48,
                Animations = [
                    new() { ID = 0, Delay = 0.06f, Loop = true, Frames = [12]},
                    new() { ID = 1, Delay = 0.06f, Loop = true, Frames = [0,1,2,3,4,5,6,7,8,9,10,11]},
                ]
            }
        );

        StandardSelectionEntry = context.Registry.MenuStates.RegisterMenuState(
            "StandardSelection",
            new()
            {
                MenuStateType = typeof(StandardSelectionFromVersus)
            }
        );

        SilverPortrait = context.Registry.Subtextures.RegisterTexture(
            "SilverPortrait",
            content.Root.GetRelativePath("Content/images/portrait/silver.png")
        );

        GoldPortrait = context.Registry.Subtextures.RegisterTexture(
            "GoldPortrait",
            content.Root.GetRelativePath("Content/images/portrait/gold.png")
        );

        AdjustCharacterOffset();

        foreach (var hookable in Hookables)
        {
            hookable.GetMethod(nameof(IHookable.Load))!.Invoke(null, [context.Harmony]);
        }

        LoadTextures(context.Registry, content);
        LoadLevels(context.Registry, content);
        OnInitialize += (_) => Inject();

        context.Events.OnBeforeModInstantiation += OnModBeforeLoaded;


        context.Harmony.Patch(
            AccessTools.DeclaredMethod(typeof(CustomLevelCategoryButton), "CreateLevelSets"),
            postfix: new HarmonyMethod(CustomLevelCategoryButton_CreateLevelSets_Postfix)
        );

        context.Registry.Commands.RegisterCommands("level_resize", new () 
        {
            Callback = LevelResizeCommand
        });
    }

    private static void LevelResizeCommand(string[] args)
    {
        if (IsWide)
        {
            return;
        }

        if (Engine.Instance.Scene is not Level levelScene)
        {
            return;
        }

        if (levelScene.Session.MatchSettings.LevelSystem is not VersusLevelSystem system)
        {
            return;
        }

        var levelID = system.VersusTowerData.GetLevelID();
        var towerEntry = TowerRegistry.VersusTowers[levelID];

        foreach (var level in towerEntry.Configuration.Levels) 
        {
            var filename = Path.GetFileName(level.Path);
            var dirName = Path.GetDirectoryName(level.Path);
            var xml = level.Xml!["level"];

            if (xml!.GetAttribute("width") == "320") 
            {
                xml.SetAttr("width", 420);
                InsertColumns(xml["BG"]!, "00000");
                InsertColumns(xml["BGTiles"]!, "-1,-1,-1,-1,-1,");
                InsertColumns(xml["Solids"]!, "00000");
                InsertColumns(xml["SolidTiles"]!, "-1,-1,-1,-1,-1,");
                foreach (XmlElement item2 in xml["Entities"]!)
                {
                    int num6 = item2.AttrInt("x");
                    num6 += 50;
                    item2.SetAttr("x", num6);
                    if (item2.Name == "Spawner") 
                    {
                        foreach (XmlElement node in item2) 
                        {
                            int nodeAttrX = node.AttrInt("x");
                            nodeAttrX += 50;
                            node.SetAttr("x", nodeAttrX);
                        }
                    }
                }

                var path = Path.Combine(AppContext.BaseDirectory, $"DumpLevels/Current/{dirName}/{filename}");
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                }

                xml.OwnerDocument.Save(path);
            }
        }

        static void InsertColumns(XmlElement xml, string insert)
        {
            if (xml == null || !(xml.InnerText != ""))
            {
                return;
            }
            string text = xml.InnerText;
            int num = 0;
            while (num < text.Length)
            {
                if (text[num] == '\n')
                {
                    num++;
                    continue;
                }
                text = text.Insert(num, insert);
                num = text.IndexOf('\n', num);
                if (num == -1)
                {
                    break;
                }
                num++;
            }
            xml.InnerText = text;
        }
    }


    private static void CustomLevelCategoryButton_CreateLevelSets_Postfix(ref List<string> __result)
    {
        if (IsWide)
        {
            __result.Clear();
            __result.AddRange(WideTowerManager.Instance.VersusLevelSets);
        }
    }

    private void OnModBeforeLoaded(object? sender, BeforeModInstantiationEventArgs e)
    {
        if (!e.Context.Interop.IsModDepends(Meta))
        {
            return;
        }

        // get levels
        if (e.ModContent.Root.TryGetRelativePath("Content/Levels/Versus", out var versusLocation)) 
        {
            foreach (var map in versusLocation.Childrens)
            {
                if (map.TryGetRelativePath("Wide", out var wideLevels))
                {
                    var levels = new List<IResourceInfo>();
                    foreach (var child in wideLevels.Childrens)
                    {
                        if (child.Path.EndsWith("oel"))
                        {
                            levels.Add(child);
                        }
                    }

                    WideTowerManager.Instance.AddEntry(
                        e.ModContent.Metadata.Name + "/" + Path.GetFileName(map.Path), 
                        [..levels]);
                }
            }
        }

        var api = Context.Interop.GetApi<IFortRiseContentApi>("FortRise.Content");
        if (api is null)
        {
            return;
        }

        var content = api.LoaderApi.GetContentConfiguration(e.ModContent.Metadata);
        if (content is null)
        {
            return;
        }

        var data = content.GetLoader("archerData");

        if (data is null || data.Path is null)
        {
            return;
        }

        var enumeratedPaths = new List<IResourceInfo>();

        foreach (var p in data.Path)
        {
            enumeratedPaths.AddRange(e.ModContent.Root.EnumerateChildrens(p));
        }

        foreach (var res in enumeratedPaths)
        {
            var r = res.Xml;

            // TODO: validation
            // TODO: alt
            // TODO: secret
            var archers = r!["Archers"]!;
            foreach (XmlElement archer in archers.GetElementsByTagName("Archer"))
            {
                SetOffsetPortrait(archer);
            }

            foreach (XmlElement archer in archers.GetElementsByTagName("AltArcher"))
            {
                SetOffsetPortrait(archer);
            }

            foreach (XmlElement archer in archers.GetElementsByTagName("SecretArcher"))
            {
                SetOffsetPortrait(archer);
            }

            void SetOffsetPortrait(XmlElement archer)
            {
                var id = archer.Attr("id");
                var offsetX = (int)archer.ChildFloat("WideNotJoinedOffsetX", 0);
                var offsetY = (int)archer.ChildFloat("WideNotJoinedOffsetY", 0);

                var joinedOffsetX = (int)archer.ChildFloat("WideJoinedOffsetX", 0);
                var joinedOffsetY = (int)archer.ChildFloat("WideJoinedOffsetY", 0);

                var isAlt = archer.Attr("Alt", string.Empty) != string.Empty;
                var isSecret = archer.Attr("Secret", string.Empty) != string.Empty;

                var archerEntry = Context.Registry.Archers.GetArcher(e.ModContent.Metadata.Name + "/" + id)!; // we are sure that this archer exists and registered

                if (isAlt)
                {
                    NotJoinedAltCharacterOffset[archerEntry.Index] = new Vector2(offsetX, offsetY);
                }
                else if (isSecret)
                {
                    NotJoinedSecretCharacterOffset[archerEntry.Index] = new Vector2(offsetX, offsetY);
                }
                else 
                {
                    NotJoinedCharacterOffset[archerEntry.Index] = new Vector2(offsetX, offsetY);
                }
            }
        }
    }

    public override object? GetApi() => new ApiImplementation();

    private static void AdjustCharacterOffset()
    {
        NotJoinedCharacterOffset[6] = new Vector2(0, 50);
    }

    private static void LoadLevels(IModRegistry registry, IModContent content)
    {
        var versusChapters = content.Root.GetRelativePath("Content/WideLevels/Versus");

        foreach (var chapter in versusChapters.Childrens)
        {
            List<IResourceInfo> levels = [];
            List<Treasure> treasures = [];
            IResourceInfo? xml = null;
            foreach (var level in chapter.Childrens)
            {
                if (level.Path.EndsWith("oel"))
                {
                    levels.Add(level);
                    continue;
                }

                if (level.Path.EndsWith("xml"))
                {
                    xml = level;
                    continue;
                }
            }

            if (xml is null)
            {
                continue;
            }

            var doc = xml.Xml!;
            var tow = doc["tower"];
            var themeName = tow!["theme"]!.InnerText.Trim();
            var treasure = tow!["treasure"];
            var csv = Calc.ReadCSV(treasure!.InnerText);
            var arrowRate = treasure.AttrFloat("arrows", 0.6f);
            var arrowShuffle = treasure.AttrBool("arrowShuffle", false);

            foreach (var c in csv)
            {
                treasures.Add(new Treasure()
                {
                    Pickup = Calc.StringToEnum<Pickups>(c)
                });
            }

            MapEntry[themeName] = registry.Towers.RegisterVersusTower(
                themeName,
                new()
                {
                    Levels = [.. levels],
                    Treasure = [.. treasures],
                    Theme = themeName,
                    SpecialArrowRate = arrowRate,
                    ArrowShuffle = arrowShuffle,
                    Procedural = tow!.HasChild("procedural")
                }
            );
        }
    }

    private static void LoadTextures(IModRegistry registry, IModContent content)
    {
        var resource = content.Root.GetRelativePath("Content/images/bg");

        foreach (var child in resource.Childrens)
        {
            registry.Subtextures.RegisterTexture(
                Path.GetFileNameWithoutExtension(child.Path),
                child,
                SubtextureAtlasDestination.BGAtlas
            );
        }

        var data = content.Root.GetRelativePath("Content/Atlas/GameData/sixPlayerBGData.xml").Xml!;

        foreach (XmlElement xml in data!.GetElementsByTagName("BG"))
        {
            WideBG.Add(xml.Attr("id"), xml);
        }
    }

    private static void Inject()
    {
        foreach (var theme in GameData.Themes.Keys)
        {
            if (theme == "GauntletA" || theme == "GauntletB")
            {
                WideRedirector[theme] = "Gauntlet";
                continue;
            }
            WideRedirector[theme] = theme;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using FortRise;
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

public class WiderSetModule : Mod
{
    public static Type[] Hookables = [
        typeof(ActorHooks),
        typeof(ArcherPortraitHooks),
        typeof(BackgroundHooks),
        typeof(EnemyHooks),
        typeof(FightButtonHooks),
        typeof(GameplayLayerHooks),
        typeof(GifExporterHooks),
        typeof(GifEncoderHooks),
        typeof(HUDFadeHooks),
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
        typeof(MainMenuHooks),
        typeof(MapSceneHooks),
        typeof(MenuBackgroundHooks),
        typeof(MenuButtonGuideHooks),
        typeof(MiasmaHooks),
        typeof(OrbLogicHooks),
        typeof(PlayerInputHooks),
        typeof(ReplayFrameHooks),
        typeof(ReplayViewerHooks),
        typeof(RollcallElementHooks),
        typeof(RoundLogicHooks),
        typeof(TFGameHooks),
        typeof(TreasureSpawnerHooks),
        typeof(ScreenHooks),
        typeof(SessionHooks),
        typeof(VariantHooks),
        typeof(VariantPerPlayerHooks),
        typeof(VersusAwardsHooks),
        typeof(VersusLevelSystemHooks),
        typeof(VersusStartHooks),
        typeof(VersusMatchResultsHooks),
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
        }
    }
    public static bool DirtyWide { get; internal set; }
    public static bool AboutToGetWide { get; internal set; }
    private static bool wide;
    public static IMenuSpriteContainerEntry StandardSetSprite { get; internal set; } = null!;
    public static IMenuSpriteContainerEntry WideSetSprite { get; internal set; } = null!;
    public static IMenuStateEntry StandardSelectionEntry { get; internal set; } = null!;

    public static Dictionary<int, Vector2> NotJoinedCharacterOffset = new Dictionary<int, Vector2>();
    public static Dictionary<int, Vector2> NotJoinedAltCharacterOffset = new Dictionary<int, Vector2>();
    public static Dictionary<string, XmlElement> WideBG = new Dictionary<string, XmlElement>();
    public static Dictionary<string, string> WideRedirector = new Dictionary<string, string>();
    public static Dictionary<string, IVersusTowerEntry> MapEntry = new Dictionary<string, IVersusTowerEntry>();

    public static WiderSetModule Instance { get; private set; } = null!;

    public WiderSetModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {

        IsWide = false;
        Instance = this;
        TFGame.Players = new bool[8];
        TFGame.Characters = new int[8];
        TFGame.AltSelect = new ArcherData.ArcherTypes[8];
        TFGame.CoOpCrowns = new bool[8];
        Session.TeamStartArrows = [3, 3, 3, 3, 2, 2, 2];

        for (int i = 4; i < 8; i++)
        {
            TFGame.Characters[i] = i;
        }
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

        AdjustCharacterOffset();

        foreach (var hookable in Hookables)
        {
            hookable.GetMethod(nameof(IHookable.Load))!.Invoke(null, [context.Harmony]);
        }

        LoadTextures(context.Registry, content);
        OnInitialize += (_) => Inject();
    }

    public override object? GetApi() => new ApiImplementation();

    private static void AdjustCharacterOffset()
    {
        NotJoinedCharacterOffset[6] = new Vector2(0, 50);
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

        var versusChapters = content.Root.GetRelativePath("Content/WideLevels/Versus");

        foreach (var chapter in versusChapters.Childrens)
        {
            List<IResourceInfo> levels = new List<IResourceInfo>();
            List<Treasure> treasures = new List<Treasure>();
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
                    Levels = levels.ToArray(),
                    Treasure = treasures.ToArray(),
                    Theme = themeName,
                    SpecialArrowRate = arrowRate,
                    ArrowShuffle = arrowShuffle,
                    Procedural = tow!.HasChild("procedural")
                }
            );
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
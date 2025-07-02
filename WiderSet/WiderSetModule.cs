using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        typeof(ArcherPortraitHooks),
        typeof(BackgroundHooks),
        typeof(FightButtonHooks),
        typeof(LevelHooks),
        typeof(MainMenuHooks),
        typeof(MapSceneHooks),
        typeof(MenuBackgroundHooks),
        typeof(PlayerInputHooks),
        typeof(RollcallElementHooks),
        typeof(TFGameHooks),
        typeof(ScreenHooks),
        typeof(ScreenTitleHooks),
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

    public static WiderSetModule Instance { get; private set; } = null!;

    public WiderSetModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        IsWide = false;
        Instance = this;
        TFGame.Players = new bool[8];
        TFGame.Characters = new int[8];
        TFGame.AltSelect = new ArcherData.ArcherTypes[8];
        TFGame.CoOpCrowns = new bool[8];

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
    }

    public override object? GetApi() => new ApiImplementation();

    private static void AdjustCharacterOffset()
    {
        NotJoinedCharacterOffset[6] = new Vector2(0, 50);
    }
}

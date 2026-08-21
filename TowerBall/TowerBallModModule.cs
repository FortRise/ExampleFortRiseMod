using System;
using FortRise;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Monocle;
using Teuria.WiderSet;
using TowerFall;

namespace TowerBall;

public class Standard : IWiderSetModApi
{
    public bool IsWide { get => false; set {} }

    public Matrix WideIdentity => throw new NotImplementedException();

    public float UIXOffset => 0;
}

public sealed class TowerBallModModule : Mod
{
    public static TowerBallModModule Instance { get; private set; } = null!;
    public static ISubtextureEntry GameModeIcon { get; private set; } = null!;
    public static IWiderSetModApi WiderSetModApi { get; private set; } = null!;

    public static int TestWideUI => WiderSetModApi.IsWide ? 100 : 0;
    public static float TestHalfWideUI => WiderSetModApi.UIXOffset;

    public TowerBallModModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;
        WiderSetModApi = context.Interop.GetApi<IWiderSetModApi>("Teuria.WiderSet") 
            ?? new Standard();

        GameModeIcon = context.Registry.Subtextures.GetTexture(
            "Suyooo.TowerBall/gamemodes/TowerBall", 
            SubtextureAtlasDestination.MenuAtlas)!;

        BasketBall.Register(context);
        TowerBallSFX.Register(content, context);
        TowerBall.Register(context);

        PlayerHooks.Patch(context.Harmony);
        
        context.Harmony.PatchAll(typeof(TowerBallModModule).Assembly);
    }
}

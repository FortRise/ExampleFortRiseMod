using FortRise;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace ColoredDrillArrows;

public class ColoredDrillArrowsModule : Mod
{
    private ISubtextureEntry drillArrow;
    private ISubtextureEntry buriedArrow;

    public ColoredDrillArrowsModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        drillArrow = context.Registry.Subtextures.RegisterTexture(
            content.Root.GetRelativePath("Content/drillArrow.png")
        );

        buriedArrow = context.Registry.Subtextures.RegisterTexture(
            content.Root.GetRelativePath("Content/drillArrowBuried.png")
        );

        context.Harmony.Patch(
            AccessTools.DeclaredMethod(typeof(DrillArrow), "InitGraphics"),
            postfix: new HarmonyMethod(DrillArrow_InitGraphics_Postfix)
        );

        OnInitialize += (context) =>
        {
            TFGame.Atlas.SubTextures["arrows/drillArrow"] = drillArrow.Subtexture;
            TFGame.Atlas.SubTextures["arrows/drillArrowBuried"] = buriedArrow.Subtexture;
        };
    }

    private static void DrillArrow_InitGraphics_Postfix(DrillArrow __instance)
    {
        var dynSelf = DynamicData.For(__instance);
        var normalSprite = dynSelf.Get<Sprite<int>>("normalSprite")!;
        var buriedImage = dynSelf.Get<Image>("buriedImage")!;
        if (__instance.CharacterIndex != -1)
        {
            normalSprite.Color = ArcherData.Archers[__instance.CharacterIndex].ColorB;
            buriedImage.Color = ArcherData.Archers[__instance.CharacterIndex].ColorB;
        }
    }
}
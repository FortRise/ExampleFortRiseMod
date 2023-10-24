using AdditionalVariants.EX;
using AdditionalVariants.EX.JesterHat;
using FortRise;
using Monocle;

namespace AdditionalVariants;

[Fort("com.terria.additionalvariants", "Additional Variants")]
public class AdditionalVariantsModule : FortModule
{
    public static Atlas AVAtlas;
    public static NeonShaderResource NeonShader;

    public override void LoadContent()
    {
        AVAtlas = Content.LoadAtlas("Atlas/atlas.json", "Atlas/atlas.png");
        NeonShader = Content.LoadShader<NeonShaderResource>("Effects/neon.fxb", "Neon", out var _);
    }

    public override void Load()
    {
        On.TowerFall.Player.Added += BottomlessQuiver;
        On.TowerFall.Level.Begin += Begin_patch;
        AtomicArrow.Load();
        InvincibleTechnomageVariantSequence.Load();
        PlayerDeathVariants.Load();
        JesterHat.Load();
        DarkWorld.Load();
        LavaOverload.Load();
        NoDodgeCancel.Load();
        ChaoticRoll.Load();
        FadingArrow.Load();
        FortRise.RiseCore.Events.OnPreInitialize += OnPreInitialize;
    }

    private void Begin_patch(On.TowerFall.Level.orig_Begin orig, TowerFall.Level self)
    {
        orig(self);
        // var filter = new NeonFilter();
        // self.Activate(filter);
    }

    public override void Unload()
    {
        On.TowerFall.Player.Added -= BottomlessQuiver;
        On.TowerFall.Level.Begin -= Begin_patch;
        NoDodgeCancel.Unload();
        AtomicArrow.Unload();
        InvincibleTechnomageVariantSequence.Unload();
        PlayerDeathVariants.Unload();
        JesterHat.Unload();
        DarkWorld.Unload();
        LavaOverload.Unload();
        ChaoticRoll.Unload();
        FadingArrow.Unload();
        FortRise.RiseCore.Events.OnPreInitialize -= OnPreInitialize;
    }

    private void OnPreInitialize()
    {
        TfExAPIModImports.RegisterVariantStateEvents?.Invoke
            (this, "JestersHat", JesterHatStateEvents.OnSaveState, JesterHatStateEvents.OnLoadState);
    }

    private void BottomlessQuiver(On.TowerFall.Player.orig_Added orig, TowerFall.Player self)
    {
        orig(self);
        if (self.Level.Session.MatchSettings.Variants.GetCustomVariant("BottomlessQuiver")[self.PlayerIndex])
            self.Arrows.SetMaxArrows(int.MaxValue);
    }

    public override void OnVariantsRegister(VariantManager manager, bool noPerPlayer = false)
    {
        var blQuiverInfo = new CustomVariantInfo(
            "BottomlessQuiver", AVAtlas["variants/bottomlessQuiver"],
            "No limit on how many arrows you can hold".ToUpperInvariant(),
            CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom
        );
        var atomicInfo = new CustomVariantInfo(
            "AtomicArrow", AVAtlas["variants/atomicArrow"],
            description: "Arrow explodes when stucked".ToUpperInvariant(),
            CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom
        );
        var annoyanceInfo = new CustomVariantInfo(
            "AnnoyingMage", AVAtlas["variants/annoyingMage"],
            description: "Summons an invincible Technomage".ToUpperInvariant(),
            CustomVariantFlags.CanRandom
        );
        var shockInfo = new CustomVariantInfo(
            "ShockDeath", AVAtlas["variants/shockDeath"],
            description: "Summons a shockwave when a player died".ToUpperInvariant(),
            CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom
        );
        var chestInfo = new CustomVariantInfo(
            "ChestDeath", AVAtlas["variants/chestDeath"],
            description: "Spawns a chest when a player died".ToUpperInvariant(),
            CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom
        );
        var jestInfo = new CustomVariantInfo(
            "JestersHat", AVAtlas["variants/jesterHat"],
            description: "Allows player to teleport by dashing".ToUpperInvariant(),
            CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom
        );
        var darkInfo = new CustomVariantInfo(
            "DarkWorld", AVAtlas["variants/darkWorld"],
            description: "Makes dark effect darker".ToUpperInvariant(),
            CustomVariantFlags.CanRandom
        );
        var lavaInfo = new CustomVariantInfo(
            "LavaOverload", AVAtlas["variants/lavaOverload"],
            description: "Four sides lava will appear".ToUpperInvariant(),
            CustomVariantFlags.CanRandom
        );
        var rollInfo = new CustomVariantInfo(
            "ChaoticRoll", AVAtlas["variants/chaoticroll"],
            description: "Random variants every round".ToUpperInvariant()
        );
        var noHypersInfo = new CustomVariantInfo(
            "NoHypers", AVAtlas["variants/noHypers"],
            description: "Removes the ability to hyper dodge".ToUpperInvariant(),
            CustomVariantFlags.CanRandom | CustomVariantFlags.PerPlayer
        );
        var noDodgeCancelInfo = new CustomVariantInfo(
            "NoDodgeCancel", AVAtlas["variants/noDodgeCancel"],
            description: "Removes the dodge cancellation mechanic".ToUpperInvariant(),
            CustomVariantFlags.CanRandom | CustomVariantFlags.PerPlayer
        );
        var fadingInfo = new CustomVariantInfo(
            "FadingArrow", AVAtlas["variants/fadingArrow"],
            description: "Arrow will start fading when stucked".ToUpperInvariant(),
            CustomVariantFlags.CanRandom | CustomVariantFlags.PerPlayer
        );

        var bottomlessQuiver = manager.AddVariant(blQuiverInfo);
        manager.AddVariant(atomicInfo);
        var annoyingMage = manager.AddVariant(annoyanceInfo);
        manager.AddVariant(shockInfo);
        manager.AddVariant(chestInfo);
        manager.AddVariant(jestInfo);
        manager.AddVariant(darkInfo);
        manager.AddVariant(lavaInfo);
        manager.AddVariant(rollInfo);
        var noHyper = manager.AddVariant(noHypersInfo);
        var noDodgeCancel = manager.AddVariant(noDodgeCancelInfo);
        manager.AddVariant(fadingInfo);

        manager.CreateLinks(bottomlessQuiver, manager.MatchVariants.NoQuivers);
        manager.CreateLinks(bottomlessQuiver, manager.MatchVariants.SmallQuivers);
        manager.CreateLinks(annoyingMage, manager.MatchVariants.DarkPortals);
        manager.CreateLinks(noHyper, manager.MatchVariants.NoDodging);
        manager.CreateLinks(noDodgeCancel, manager.MatchVariants.NoDodging);
        manager.CreateLinks(noHyper, noDodgeCancel);
    }
}

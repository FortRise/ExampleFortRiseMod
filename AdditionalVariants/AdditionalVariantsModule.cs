using AdditionalVariants.EX;
using AdditionalVariants.EX.JesterHat;
using FortRise;
using Monocle;

namespace AdditionalVariants;

[Fort("com.terria.additionalvariants", "Additional Variants")]
public class AdditionalVariantsModule : FortModule
{
    public static NeonShaderResource NeonShader;

    public override void LoadContent()
    {
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
        PlayerStamina.Load();
        KingsWrath.Load();
        NoArrowTinks.Load();
        DrillingArrow.Load();
        UnfairAutobalance.Load();
        // ArrowFallingUp.Load();
        FortRise.RiseCore.Events.OnPreInitialize += OnPreInitialize;
    }

    private void Begin_patch(On.TowerFall.Level.orig_Begin orig, TowerFall.Level self)
    {
        orig(self);
        if (self.Session.MatchSettings.Variants.GetCustomVariant("AdditionalVariants/NeonWorld")) 
        {
            var filter = new NeonFilter();
            self.Activate(filter);
        }
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
        PlayerStamina.Unload();
        KingsWrath.Unload();
        NoArrowTinks.Unload();
        DrillingArrow.Unload();
        UnfairAutobalance.Unload();
        // ArrowFallingUp.Unload();
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
        if (self.Level.Session.MatchSettings.Variants.GetCustomVariant("AdditionalVariants/BottomlessQuiver")[self.PlayerIndex])
            self.Arrows.SetMaxArrows(int.MaxValue);
    }

    public override void OnVariantsRegister(VariantManager manager, bool noPerPlayer = false)
    {
        var blQuiverInfo = new CustomVariantInfo(
            "BottomlessQuiver", TextureRegistry.BottomlessQuiver,
            "No limit on how many arrows you can hold".ToUpperInvariant(),
            CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom
        );
        var atomicInfo = new CustomVariantInfo(
            "AtomicArrow", TextureRegistry.AtomicArrow,
            description: "Arrows explode when they hit a wall".ToUpperInvariant(),
            CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom
        );
        var annoyanceInfo = new CustomVariantInfo(
            "AnnoyingMage", TextureRegistry.AnnoyingMage,
            description: "Summons an invincible Technomage".ToUpperInvariant(),
            CustomVariantFlags.CanRandom
        );
        var shockInfo = new CustomVariantInfo(
            "ShockDeath", TextureRegistry.ShockDeath,
            description: "Summons a shockwave when a player dies".ToUpperInvariant(),
            CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom
        );
        var chestInfo = new CustomVariantInfo(
            "ChestDeath", TextureRegistry.ChestDeath,
            description: "Spawns a chest when a player dies".ToUpperInvariant(),
            CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom
        );
        var jestInfo = new CustomVariantInfo(
            "JestersHat", TextureRegistry.JesterHat,
            description: "Allows players to teleport by dashing".ToUpperInvariant(),
            CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom
        );
        var darkInfo = new CustomVariantInfo(
            "DarkWorld", TextureRegistry.DarkWorld,
            description: "Makes dark effect darker".ToUpperInvariant(),
            CustomVariantFlags.CanRandom
        );
        var lavaInfo = new CustomVariantInfo(
            "LavaOverload", TextureRegistry.LavaOverload,
            description: "Lava will appear on all four sides".ToUpperInvariant(),
            CustomVariantFlags.CanRandom
        );
        var rollInfo = new CustomVariantInfo(
            "ChaoticRoll", TextureRegistry.ChaoticRoll,
            description: "Random variants every round".ToUpperInvariant()
        );
        var noHypersInfo = new CustomVariantInfo(
            "NoHypers", TextureRegistry.NoHypers,
            description: "Removes the ability to hyper dodge".ToUpperInvariant(),
            CustomVariantFlags.CanRandom | CustomVariantFlags.PerPlayer
        );
        var noDodgeCancelInfo = new CustomVariantInfo(
            "NoDodgeCancel", TextureRegistry.NoDodgeCancel,
            description: "Removes the dodge cancellation mechanic".ToUpperInvariant(),
            CustomVariantFlags.CanRandom | CustomVariantFlags.PerPlayer
        );
        var fadingInfo = new CustomVariantInfo(
            "FadingArrow", TextureRegistry.FadingArrow,
            description: "Arrow will start fading when stuck to a wall".ToUpperInvariant(),
            CustomVariantFlags.CanRandom | CustomVariantFlags.PerPlayer
        );
        var neonInfo = new CustomVariantInfo(
            "NeonWorld", TextureRegistry.NeonWorld,
            description: "Welcome to the Cyber World".ToUpperInvariant(),
            CustomVariantFlags.CanRandom
        );
        var staminaInfo = new CustomVariantInfo(
            "DashStamina", TextureRegistry.DashStamina,
            description: "Adds a stamina mechanic for dashes".ToUpperInvariant(),
            CustomVariantFlags.CanRandom | CustomVariantFlags.PerPlayer
        );
        var kingsWrathInfo = new CustomVariantInfo(
            "KingsWrath", TextureRegistry.KingsWrath,
            description: "Crown drops on the floor will spawn Lethal Ghost".ToUpperInvariant(),
            CustomVariantFlags.CanRandom | CustomVariantFlags.PerPlayer
        );
        var noArrowTinksInfo = new CustomVariantInfo(
            "NoArrowTinks", TextureRegistry.NoArrowTinks,
            description: "Disabled collision of an arrow to arrow".ToUpperInvariant(),
            CustomVariantFlags.CanRandom
        );
        var drillingArrowInfo = new CustomVariantInfo(
            "DrillingArrow", TextureRegistry.DrillingArrow,
            description: "Make all arrows act like a Drill Arrow".ToUpperInvariant(),
            CustomVariantFlags.CanRandom | CustomVariantFlags.PerPlayer
        );
        var unfairAutobalanceInfo = new CustomVariantInfo(
            "UnfairAutobalance", TextureRegistry.UnfairAutobalance,
            description: "Top players always get an advantage".ToUpperInvariant(),
            CustomVariantFlags.CanRandom
        );
        // var arrowFallingUp = new CustomVariantInfo(
        //     "ArrowFallingUp", TextureRegistry.ArrowFallingUp,
        //     description: "Arrow will fall upwards".ToUpperInvariant(),
        //     CustomVariantFlags.CanRandom | CustomVariantFlags.PerPlayer
        // );

        var bottomlessQuiver = manager.AddVariant(blQuiverInfo);
        var atomicArrow = manager.AddVariant(atomicInfo);
        var drillingArrow = manager.AddVariant(drillingArrowInfo);
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
        var neonWorld = manager.AddVariant(neonInfo);
        var staminaDash = manager.AddVariant(staminaInfo);
        manager.AddVariant(kingsWrathInfo);
        manager.AddVariant(noArrowTinksInfo);
        var unfairAutobalance = manager.AddVariant(unfairAutobalanceInfo);
        // manager.AddVariant(arrowFallingUp);

        // manager.CreateLinks(atomicArrow, drillingArrow);
        manager.CreateLinks(bottomlessQuiver, manager.MatchVariants.NoQuivers);
        manager.CreateLinks(bottomlessQuiver, manager.MatchVariants.SmallQuivers);
        manager.CreateLinks(annoyingMage, manager.MatchVariants.DarkPortals);
        manager.CreateLinks(noHyper, manager.MatchVariants.NoDodging);
        manager.CreateLinks(noDodgeCancel, manager.MatchVariants.NoDodging);
        manager.CreateLinks(staminaDash, manager.MatchVariants.NoDodging);
        manager.CreateLinks(noHyper, noDodgeCancel);
        manager.CreateLinks(neonWorld, manager.MatchVariants.AlwaysDark);
        manager.CreateLinks(unfairAutobalance, manager.MatchVariants.NoAutobalance);
        manager.CreateLinks(unfairAutobalance, manager.MatchVariants.WeakAutobalance);
    }
}

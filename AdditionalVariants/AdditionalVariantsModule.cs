using FortRise;
using Monocle;
using TowerFall;

namespace AdditionalVariants;

[Fort("com.terria.additionalvariants", "Additional Variants")]
public class AdditionalVariantsModule : FortModule
{
    private static Atlas AVAtlas;

    public override void LoadContent()
    {
        AVAtlas = Content.LoadAtlas("Atlas/atlas.json", "Atlas/atlas.png");
    }

    public override void Load()
    {
        On.TowerFall.Player.Added += BottomlessQuiver;
        AtomicArrow.Load();
        InvincibleTechnomageVariantSequence.Load();
        PlayerDeathVariants.Load();
        JesterHat.Load();
        DarkWorld.Load();
        LavaOverload.Load();
    }

    public override void Unload()
    {
        On.TowerFall.Player.Added -= BottomlessQuiver;
        AtomicArrow.Unload();
        InvincibleTechnomageVariantSequence.Unload();
        PlayerDeathVariants.Unload();
        JesterHat.Unload();
        DarkWorld.Unload();
        LavaOverload.Unload();
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
        var bottomlessQuiver =  manager.AddVariant(blQuiverInfo);
        manager.AddVariant(atomicInfo);
        var annoyingMage = manager.AddVariant(annoyanceInfo);
        manager.AddVariant(shockInfo);
        manager.AddVariant(chestInfo);
        manager.AddVariant(jestInfo);
        manager.AddVariant(darkInfo);
        manager.AddVariant(lavaInfo);

        manager.CreateLinks(bottomlessQuiver, manager.MatchVariants.NoQuivers);
        manager.CreateLinks(bottomlessQuiver, manager.MatchVariants.SmallQuivers);
        manager.CreateLinks(annoyingMage, manager.MatchVariants.DarkPortals);
    }
}

using System;
using System.Collections;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class InvincibleTechnomage : TechnoMage, IRegisterable
{
    public static IEnemyEntry Metadata = null!;
    public static void Register(IModRegistry registry)
    {
        Metadata = registry.Enemies.RegisterEnemy("InvincibleMage", new() 
        {
            Name = "Invincible Mage",
            Loader = (position, facing, _) => new InvincibleTechnomage(position, facing)
        });
    }

    public InvincibleTechnomage(Vector2 position, Facing facing) : base(position, facing)
    {
    }

    public override bool OnArrowHit(Arrow arrow)
    {
        arrow.EnterFallMode(true, false, true);
        return true;
    }

    public override void Hurt(Vector2 force, int damage, int killerIndex, Arrow? arrow = null, Explosion? explosion = null, ShockCircle? shock = null)
    {
        Speed = force;
        if (arrow is not null)
        {
            arrow.EnterFallMode(true, false, true);
        }
    }
}

public class InvincibleTechnomageVariantSequence : Entity, IHookable
{
    public static void Load() 
    {
        On.TowerFall.Session.OnLevelLoadFinish += SpawnThisSequence;
    }

    public static void Unload() 
    {
        On.TowerFall.Session.OnLevelLoadFinish -= SpawnThisSequence;
    }

    private static void SpawnThisSequence(On.TowerFall.Session.orig_OnLevelLoadFinish orig, Session self)
    {
        orig(self);
        if (Variants.AnnoyingMage.IsActive() && self.MatchSettings == MainMenu.VersusMatchSettings)
        {
            self.CurrentLevel.Add(new InvincibleTechnomageVariantSequence());
        }
    }

    private Level level = null!;

    public InvincibleTechnomageVariantSequence()
        : base(0)
    {
        Add(new Coroutine(Sequence()));
    }

    public override void Added()
    {
        base.Added();
        level = (Scene as Level)!;
    }

    private IEnumerator Sequence()
    {
        while (!level.Session.RoundLogic.RoundStarted)
            yield return 0;
        
        Random random = new Random();
        yield return 20;
        var xmlpositions = level.GetXMLPositions("Spawner");
        if (xmlpositions.Count == 0)
        {
            RemoveSelf();
            yield break;
        }
        xmlpositions.Shuffle(random);
        var portal = new QuestSpawnPortal(xmlpositions[0], null);
        level.Add(portal);
        portal.Appear();
        yield return 20;
        portal.SpawnEnemy(InvincibleTechnomage.Metadata.Name);
        yield return 10;
        portal.ForceDisappear();
    }
}
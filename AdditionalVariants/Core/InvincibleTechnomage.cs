using System;
using System.Collections;
using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class InvincibleTechnomage : TechnoMage, IRegisterable
{
    public static IEnemyEntry Metadata = null!;
    public static void Register(IModContent content, IModRegistry registry)
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
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Session), nameof(Session.OnLevelLoadFinish)),
            postfix: new HarmonyMethod(Session_OnLevelLoadFinish_Postfix)
        );
    }

    private static void Session_OnLevelLoadFinish_Postfix(Session __instance)
    {
        if (Variants.AnnoyingMage.IsActive() && __instance.MatchSettings == MainMenu.VersusMatchSettings)
        {
            __instance.CurrentLevel.Add(new InvincibleTechnomageVariantSequence());
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
        portal.SpawnEnemy(InvincibleTechnomage.Metadata.ID);
        yield return 10;
        portal.ForceDisappear();
    }
}
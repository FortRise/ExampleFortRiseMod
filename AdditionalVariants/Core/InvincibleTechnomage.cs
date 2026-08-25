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

    internal const int PHASE_WAIT_ROUND_START = 0;
    internal const int PHASE_WAIT_APPEAR = 1;
    internal const int PHASE_PORTAL_APPEARING = 2;
    internal const int PHASE_ENEMY_SPAWNED = 3;
    internal const int PHASE_DONE = 4;

    private const float APPEAR_DELAY = 20f;
    private const float SPAWN_DELAY = 20f;
    private const float DISAPPEAR_DELAY = 10f;

    internal int Phase;
    internal float Counter;

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
    }

    public override void Added()
    {
        base.Added();
        level = (Scene as Level)!;
    }

    //A coroutine cannot be snapshotted
    //Same as before but updated to be snapshotable
    public override void Update()
    {
        base.Update();

        switch (Phase)
        {
            case PHASE_WAIT_ROUND_START:
                if (level.Session.RoundLogic.RoundStarted)
                {
                    Phase = PHASE_WAIT_APPEAR;
                    Counter = 0f;
                }
                break;

            case PHASE_WAIT_APPEAR:
                Counter += Engine.TimeMult;
                if (Counter >= APPEAR_DELAY)
                {
                    SpawnPortal();
                }
                break;

            case PHASE_PORTAL_APPEARING:
                Counter += Engine.TimeMult;
                if (Counter >= SPAWN_DELAY)
                {
                    FindPortal()?.SpawnEnemy(InvincibleTechnomage.Metadata.ID);
                    Phase = PHASE_ENEMY_SPAWNED;
                    Counter = 0f;
                }
                break;

            case PHASE_ENEMY_SPAWNED:
                Counter += Engine.TimeMult;
                if (Counter >= DISAPPEAR_DELAY)
                {
                    FindPortal()?.ForceDisappear();
                    Phase = PHASE_DONE;
                    Counter = 0f;
                }
                break;
        }
    }

    private void SpawnPortal()
    {
        var xmlpositions = level.GetXMLPositions("Spawner");
        if (xmlpositions.Count == 0)
        {
            Phase = PHASE_DONE;
            return;
        }

        TfStateInterop.RegisterRng();
        try
        {
            xmlpositions.Shuffle(Calc.Random);
        }
        finally
        {
            TfStateInterop.UnregisterRng();
        }

        var portal = new QuestSpawnPortal(xmlpositions[0], null);
        level.Add(portal);
        portal.Appear();

        Phase = PHASE_PORTAL_APPEARING;
        Counter = 0f;
    }

    //TF.State delete-recreates the portal on load
    //Instead lets find it
    private QuestSpawnPortal? FindPortal()
    {
        foreach (var layer in level.Layers.Values)
        {
            foreach (var entity in layer.Entities)
            {
                if (entity is QuestSpawnPortal portal)
                {
                    return portal;
                }
            }
        }

        return null;
    }
}
using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class UnfairAutobalance : IHookable
{
    public static void Load()
    {
        IL.TowerFall.Session.GetSpawnArrows += GetSpawnArrows_patch;
        IL.TowerFall.Session.GetSpawnShield += GetSpawnShield_patch;
    }

    public static void Unload()
    {
        IL.TowerFall.Session.GetSpawnArrows -= GetSpawnArrows_patch;
        IL.TowerFall.Session.GetSpawnShield -= GetSpawnShield_patch;
    }

    private static void GetSpawnArrows_patch(ILContext il)
    {
        var cursor = new ILCursor(il);

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(6)))
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<int, Session, int>>((arrows, session) => {
                if (Variants.UnfairAutobalance.IsActive() && HasOnePlayerCrown(session))
                {
                    return 4;
                }

                return arrows;
            });
        }

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4)))
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<int, Session, int>>((arrows, session) => {
                if (Variants.UnfairAutobalance.IsActive())
                {
                    return 6;
                }

                return arrows;
            });
        }

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(3)))
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<int, Session, int>>((arrows, session) => {
                if (Variants.UnfairAutobalance.IsActive() && HasOnePlayerCrown(session))
                {
                    return 2;
                }

                return arrows;
            });
        }

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt("System.Math", "System.Int32 Max(System.Int32,System.Int32)")))
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<int, Session, int>>((arrows, session) => {
                if (Variants.UnfairAutobalance.IsActive())
                {
                    return 3;
                }

                return arrows;
            });
        }
    }

    private static void GetSpawnShield_patch(ILContext il)
    {
        var cursor = new ILCursor(il);

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCgt()))
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate<Func<bool, Session, int, bool>>((isLosing, session, playerIndex) => {
                if (Variants.UnfairAutobalance.IsActive() && HasOnePlayerCrown(session))
                {
                    if (session.GetSpawnHatState(playerIndex) == Player.HatStates.Crown)
                    {
                        return false;
                    }

                    return true;
                }

                return isLosing;
            });
        }
    }

    private static bool HasOnePlayerCrown(Session session)
    {
        var amount = TFGame.PlayerAmount;

        for (int i = 0; i < amount; i++)
        {
            if (session.GetSpawnHatState(i) == Player.HatStates.Crown)
            {
                return true;
            }
        }

        return false;
    }
}
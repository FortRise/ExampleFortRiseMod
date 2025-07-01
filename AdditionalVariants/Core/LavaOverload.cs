using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Monocle;
using TowerFall;

namespace Teuria.AdditionalVariants;


public class LavaOverload : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LavaControl), nameof(LavaControl.Added)),
            new HarmonyMethod(LavaControl_Added_Prefix)
        );

        Harmony.ReversePatch(
            AccessTools.DeclaredMethod(typeof(Entity), nameof(Entity.Added)),
            new HarmonyMethod(Entity_Added_ReversePatch)
        );

        Harmony.ReversePatch(
            AccessTools.DeclaredMethod(typeof(LavaControl), nameof(LavaControl.Added)),
            new HarmonyMethod(LavaControl_Added_ReversePatch_Volume),
            transpiler: AccessTools.DeclaredMethod(typeof(LavaOverload), nameof(LavaControl_Added_ReversePatch_Volume_Transpiler))
        );

        Harmony.ReversePatch(
            AccessTools.DeclaredMethod(typeof(LavaControl), nameof(LavaControl.Added)),
            new HarmonyMethod(LavaControl_Added_ReversePatch_FourLava),
            transpiler: AccessTools.DeclaredMethod(typeof(LavaOverload), nameof(LavaControl_Added_ReversePatch_FourLava_Transpiler))
        );
    }

    private static void Entity_Added_ReversePatch(Entity original)
    {
        throw new NotImplementedException();
    }

    private static void LavaControl_Added_ReversePatch_FourLava(LavaControl control)
    {
        throw new NotImplementedException();
    }

    private static IEnumerable<CodeInstruction> LavaControl_Added_ReversePatch_FourLava_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            [
                ILMatch.Ldarg(0),
                ILMatch.LdcI4(4),
            ]
        );

        var index = cursor.Index;

        cursor.GotoNext(MoveType.After, [ILMatch.Pop()]);
        cursor.GotoNext(MoveType.After, [ILMatch.Pop()]);
        cursor.GotoNext(MoveType.After, [ILMatch.Pop()]);
        cursor.GotoNext(MoveType.After, [ILMatch.Pop()]);

        var instr = cursor.Instructions.ToList()[index..cursor.Index];
        instr.Add(new CodeInstruction(OpCodes.Ret));
        return instr;
    }

    private static void LavaControl_Added_ReversePatch_Volume(LavaControl control)
    {
        throw new NotImplementedException();
    }

    private static IEnumerable<CodeInstruction> LavaControl_Added_ReversePatch_Volume_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            [
                ILMatch.Ldsfld("sfx_lavaLoop")
            ]
        );

        var index = cursor.Index;

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Callvirt("Play")
            ]
        );

        var instr = cursor.Instructions.ToList()[index..cursor.Index];
        instr.Add(new CodeInstruction(OpCodes.Ret));
        return instr;
    }

    private static bool LavaControl_Added_Prefix(TowerFall.LavaControl __instance)
    {
        if (Variants.LavaOverload.IsActive())
        {
            Entity_Added_ReversePatch(__instance);

            LavaControl_Added_ReversePatch_Volume(__instance);
            LavaControl_Added_ReversePatch_FourLava(__instance);

            return false;
        }

        return true;
    }
}
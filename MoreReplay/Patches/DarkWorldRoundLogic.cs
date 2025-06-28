using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Monocle;
using TowerFall;

namespace Teuria.MoreReplay;

public class DarkWorldRoundLogicPatch : IHookable
{
    public static void Load(IHarmony harmony)
    {
        Harmony.ReversePatch(
            AccessTools.DeclaredMethod(typeof(DarkWorldRoundLogic), nameof(DarkWorldRoundLogic.OnPlayerDeath)),
            new HarmonyMethod(DarkWorldRoundLogic_OnPlayerDeath_ReversePatch),
            transpiler: AccessTools.DeclaredMethod(typeof(DarkWorldRoundLogicPatch), nameof(Transpiler)),
            null
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(DarkWorldRoundLogic), nameof(DarkWorldRoundLogic.OnPlayerDeath)),
            transpiler: new HarmonyMethod(DarkWorldRoundLogic_OnPlayerDeath_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Call("FinalKillNoSpotlightOrMusicStop")
            ]
        );

        var instr = cursor.Instructions.ToArray()[cursor.Index..];
        return instr;
    }

    private static void DarkWorldRoundLogic_OnPlayerDeath_ReversePatch(RoundLogic self)
    {
        throw new NotImplementedException();
    }

    private static IEnumerable<CodeInstruction> DarkWorldRoundLogic_OnPlayerDeath_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var get_Session = typeof(TowerFall.RoundLogic).GetProperty("Session")!.GetGetMethod()!;
        var CurrentLevel = typeof(TowerFall.Session).GetField("CurrentLevel")!;
        var get_ReplayRecorder = typeof(TowerFall.Level).GetProperty("ReplayRecorder")!.GetGetMethod()!;

        var cursor = new ILTranspilerCursor(generator, instructions);
        var br_Replay = cursor.CreateLabel();
        var br_Ret = cursor.CreateLabel();

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Stfld("Ending")
            ]
        );

        cursor.Emits(
            [
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, get_Session),
                new CodeInstruction(OpCodes.Ldfld, CurrentLevel),
                new CodeInstruction(OpCodes.Call, get_ReplayRecorder),
                new CodeInstruction(OpCodes.Brtrue_S, br_Replay)
            ]
        );

        cursor.GotoNext(
            MoveType.After,
            [ILMatch.Pop()]
        );
        cursor.Emit(new CodeInstruction(OpCodes.Br_S, br_Ret));

        // Harmony seems to ignore nop instructions for putting nop label
        // but newly created nop seems to be noticed somehow..
        CodeInstruction instruction;
        cursor.Emit(instruction = new CodeInstruction(OpCodes.Nop));
        cursor.MarkLabel(br_Replay, instruction);

        cursor.Emit(new CodeInstruction(OpCodes.Ldarg_0));

        cursor.EmitDelegate((DarkWorldRoundLogic self) =>
        {
            var entity = new Entity();
            Alarm.Set(entity, 90, () =>
            {
                entity.RemoveSelf();
                self.Session.CurrentLevel.ReplayRecorder.End();
                self.Session.CurrentLevel.ReplayViewer.Watch(self.Session.CurrentLevel.ReplayRecorder, ReplayViewer.ReplayType.Rewind, () =>
                {
                    ScreenEffects.Reset();
                    self.Session.CurrentLevel.OrbLogic.CancelSlowMo();
                    self.Session.CurrentLevel.Frozen = false;
                    DarkWorldRoundLogic_OnPlayerDeath_ReversePatch(self);
                });
            });
            self.Session.CurrentLevel.Add(entity);
        });

        cursor.MarkLabel(br_Ret, cursor.Instruction);

        return cursor.Generate();
    }
}
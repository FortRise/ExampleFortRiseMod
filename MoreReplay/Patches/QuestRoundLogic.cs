using System;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using TowerFall;

namespace Teuria.MoreReplay;

public class QuestRoundLogicPatch : IHookable
{
    public static void Load() 
    {
        IL.TowerFall.QuestRoundLogic.OnPlayerDeath += OnPlayerDeath_patch;
    }

    public static void Unload() 
    {
        IL.TowerFall.QuestRoundLogic.OnPlayerDeath -= OnPlayerDeath_patch;
    }


    private static void OnPlayerDeath_patch(ILContext ctx)
    {
        var get_Session = typeof(TowerFall.RoundLogic).GetProperty("Session")!.GetGetMethod()!;
        var CurrentLevel = typeof(TowerFall.Session).GetField("CurrentLevel")!;
        var get_ReplayRecorder = typeof(TowerFall.Level).GetProperty("ReplayRecorder")!.GetGetMethod()!;

        var cursor = new ILCursor(ctx);
        var br_Replay = cursor.DefineLabel();
        var br_Ret = cursor.DefineLabel();

        // TODO TryGotoNext
        cursor.GotoNext(MoveType.After, instr => instr.MatchStfld("TowerFall.Level", "Ending"));

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Call, get_Session);
        cursor.Emit(OpCodes.Ldfld, CurrentLevel);
        cursor.Emit(OpCodes.Call, get_ReplayRecorder);
        cursor.Emit(OpCodes.Brtrue_S, br_Replay);

        cursor.GotoNext(MoveType.After, instr => instr.MatchPop());
        cursor.Emit(OpCodes.Br_S, br_Ret);

        cursor.MarkLabel(br_Replay);
        cursor.Emit(OpCodes.Ldarg_0);

        cursor.EmitDelegate<Action<QuestRoundLogic>>((self) => {
            var entity = new Entity();
            Alarm.Set(entity, 90, () => {
                entity.RemoveSelf();
                self.Session.CurrentLevel.ReplayRecorder.End();
                self.Session.CurrentLevel.ReplayViewer.Watch(self.Session.CurrentLevel.ReplayRecorder, ReplayViewer.ReplayType.Rewind, () => {
                    ScreenEffects.Reset();
                    self.Session.CurrentLevel.OrbLogic.CancelSlowMo();
                    self.Session.CurrentLevel.Ending = true;
                    self.Session.CurrentLevel.Frozen = false;
                    self.Session.CurrentLevel.Add<QuestGameOver>(new QuestGameOver(self));
                });
            });
            self.Session.CurrentLevel.Add(entity);
        });

        cursor.MarkLabel(br_Ret);
    }
}
using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.MoreReplay;

public class ReplayRecorderPatch : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(ReplayRecorder), nameof(ReplayRecorder.RecordRender)),
            transpiler: new HarmonyMethod(Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(210)]);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((int x, ReplayRecorder recorder) =>
        {
            var level = Private.Field<ReplayRecorder, Level>("level", recorder).Read();
            if (level.Session.MatchSettings.Mode == Modes.Trials)
            {
                return MoreReplayModule.Settings.TrialsFrameCount;
            }
            return MoreReplayModule.Settings.FrameCount;
        });

        return cursor.Generate();
    }
}

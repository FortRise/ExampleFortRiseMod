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
        cursor.EmitDelegate((int x) => MoreReplayModule.Settings.FrameCount);

        return cursor.Generate();
    }
}

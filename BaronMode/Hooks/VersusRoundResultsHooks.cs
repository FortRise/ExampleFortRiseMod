using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Monocle;
using Teuria.BaronMode.GameModes;
using TowerFall;

namespace Teuria.BaronMode;

internal sealed class VersusRoundResultsHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(VersusRoundResults), nameof(VersusRoundResults.Update)),
            transpiler: new HarmonyMethod(VersusRoundResults_Update_Transpiler)
        );

        harmony.Patch(
            AccessTools.EnumeratorMoveNext(
                AccessTools.DeclaredMethod(typeof(VersusRoundResults), "Sequence")
            ),
            transpiler: new HarmonyMethod(VersusRoundResults_Sequence_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> VersusRoundResults_Update_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.Brtrue()]);
        var operand = cursor.Prev.operand;
        cursor.Emit(new CodeInstruction(OpCodes.Ldarg_0));
        cursor.EmitDelegate((VersusRoundResults __instance) => __instance.Level.Session.MatchSettings.Mode == Baron.BaronGameMode.Modes);
        cursor.Emit(OpCodes.Brtrue, operand);

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> VersusRoundResults_Sequence_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var sequenceMethod = AccessTools.EnumeratorMoveNext(
            AccessTools.DeclaredMethod(typeof(VersusRoundResults), "Sequence")
        );

        var fiveEight = sequenceMethod.DeclaringType!.GetField("<i>5__8", BindingFlags.NonPublic | BindingFlags.Instance);
        var self = sequenceMethod.DeclaringType!.GetField("<>4__this");

        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.Callvirt("Invoke")]);
        cursor.GotoNext(MoveType.After, [ILMatch.Callvirt("Invoke")]);

        cursor.Emit(new CodeInstruction(OpCodes.Ldarg_0));
        cursor.Emit(new CodeInstruction(OpCodes.Ldfld, self));

        cursor.Emit(new CodeInstruction(OpCodes.Ldarg_0));
        cursor.Emit(new CodeInstruction(OpCodes.Ldfld, fiveEight));
        cursor.EmitDelegate((Sprite<int> sprite, VersusRoundResults __instance, int fiveEight) => 
        {
            if (__instance.Level.Session.MatchSettings.Mode != Baron.BaronGameMode.Modes)
            {
                return sprite;
            }

            var archerData = ArcherData.Get(TFGame.Characters[fiveEight], TFGame.AltSelect[fiveEight]);
            Sprite<int> spriteInt = TFGame.SpriteData.GetSpriteInt(archerData.Gems.Gameplay);
            return spriteInt;
        });

        return cursor.Generate();
    }
}

using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using TowerFall;

namespace Teuria.WiderSet;

public class VariantToggleHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(VariantToggle), "Render"),
            transpiler: new HarmonyMethod(typeof(VariantToggleHooks), nameof(VariantToggle_Render_Transpiler))
        );
    }

    private static int WideLoopEnd(int vanillaEnd) => WiderSetModule.IsWide ? 8 : vanillaEnd;

    private static int BannerCol(int i) => (WiderSetModule.IsWide && i >= 4) ? (i - 4) : i;

    private static float BannerYOffset(float vanilla, int i)
        => (WiderSetModule.IsWide && i >= 4) ? 18f : vanilla;

    private static void EmitSameLdloc(ILTranspilerCursor cursor, CodeInstruction ldlocTemplate)
    {
        // Copies exactly the same ldloc opcode/operand variant (ldloc.0, ldloc.s, ldloc, etc.)
        cursor.Emit(ldlocTemplate.opcode, ldlocTemplate.operand);
    }

    private static IEnumerable<CodeInstruction> VariantToggle_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        // 1) Capture the loop index local load instruction (ldloc.*)
        // and widen the loop bound from 4 -> (IsWide ? 8 : 4)
        cursor.Reset();

        // Find: ldloc.* ; ldc.i4.4  (this should be the loop compare i < 4)
        cursor.GotoNext(MoveType.Before, [
            ILMatch.Ldloc(),
            ILMatch.LdcI4(4)
        ]);

        // Save the exact ldloc instruction used for i
        var ldlocI = cursor.Instruction; // currently at ldloc.*

        // Move after the ldc.i4.4 so we can rewrite the constant
        cursor.GotoNext(MoveType.After, [
            ILMatch.Ldloc(),
            ILMatch.LdcI4(4)
        ]);

        // Stack has the loop bound (4). Replace with IsWide ? 8 : 4.
        cursor.EmitDelegate((int end) => WideLoopEnd(end));

        // 2) Patch banner draw positioning only inside the playerBanner block
        cursor.Reset();

        while (true)
        {
            cursor.GotoNext(MoveType.After, out bool foundBanner, [
                ILMatch.Ldstr("variants/playerBanner")
            ]);

            if (!foundBanner)
            {
                break;
            }

            int bannerAnchorIndex = cursor.Index;

            // 2a) Patch X: the "5 * i" part -> "5 * BannerCol(i)"
            cursor.GotoNext(MoveType.After, out bool foundX, [
                ILMatch.LdcI4(5),
                ILMatch.Ldloc()
            ]);

            if (foundX)
            {
                // We are right after the ldloc i that will become part of (5 * i).
                // Rewrite i -> BannerCol(i) (only affects position).
                cursor.EmitDelegate((int i) => BannerCol(i));
            }
            else
            {
                // If IL differs in this build, restore cursor near banner anchor and continue;
                cursor.Index = bannerAnchorIndex;
            }

            // 2b) Patch Y: base.Y + 9f -> base.Y + BannerYOffset(9f, i)

            // Find the first ldc.r4 9f after the banner anchor (within this block).
            cursor.GotoNext(MoveType.After, out bool foundY, [
                ILMatch.LdcR4(9f)
            ]);

            if (foundY)
            {
                // Stack currently has 9f.
                // We need i too, so load the same loop local (ldloc.*) we captured earlier,
                // then call delegate (float vanilla, int i) -> float.
                EmitSameLdloc(cursor, ldlocI);
                cursor.EmitDelegate((float vanilla, int i) => BannerYOffset(vanilla, i));
            }

            // Move forward a bit so we don’t get stuck matching the same banner forever.
            cursor.Index = bannerAnchorIndex + 1;
        }

        return cursor.Generate();
    }
}
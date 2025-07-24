using System.Reflection.Emit;
using FortRise.Transpiler;

namespace Teuria.WiderSet;

internal static class ILMatchExt
{
    public static InstructionMatcher Ble_Un_S() // fortrise does not have this yet
    {
        return new InstructionMatcher(instr => instr.opcode == OpCodes.Ble_Un_S);
    }
}
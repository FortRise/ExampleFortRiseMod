using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise.Transpiler;
using HarmonyLib;

namespace Teuria.WiderSet;

internal static class ScreenWidthUtilities
{
    // public static IEnumerable<CodeInstruction> ScreenWidthTranspilerNoCondition(
    //     IEnumerable<CodeInstruction> instructions,
    //     ILGenerator generator)
    // {
    //     var cursor = new ILTranspilerCursor(generator, instructions);

    //     cursor.Encompass(x =>
    //     {
    //         while (x.Next(
    //                 MoveType.After,
    //                 [
    //                     ILMatch.LdcI4(320)
    //                 ]
    //             )
    //         )
    //         {
    //             cursor.EmitDelegate((int playerAmount) =>
    //             {
    //                 if (WiderSetModule.IsWide)
    //                 {
    //                     return playerAmount + 4;
    //                 }

    //                 return playerAmount;
    //             });
    //         }
    //     });


    //     return cursor.Generate();
    // }
}

internal static class PlayerAmountUtilities
{
    public static IEnumerable<CodeInstruction> EightPlayerTranspiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            while (x.Next(
                    MoveType.After,
                    [
                        ILMatch.LdcI4(4)
                    ]
                )
            )
            {
                cursor.EmitDelegate((int playerAmount) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return playerAmount + 4;
                    }

                    return playerAmount;
                });
            }
        });


        return cursor.Generate();
    }

    public static IEnumerable<CodeInstruction> EightPlayerTranspilerNoCondition(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            while (x.Next(
                    MoveType.After,
                    [
                        ILMatch.LdcI4(4)
                    ]
                )
            )
            {
                cursor.EmitDelegate((int playerAmount) =>
                {
                    return playerAmount + 4;
                });
            }
        });


        return cursor.Generate();
    }
}
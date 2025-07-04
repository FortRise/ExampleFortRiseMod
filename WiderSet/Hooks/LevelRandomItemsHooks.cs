using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class LevelRandomItemsHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LevelRandomItems), "Collide", [typeof(bool[,]), typeof(Rectangle)]),
            transpiler: new HarmonyMethod(LevelRandomItems_Collide_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LevelRandomItems), nameof(LevelRandomItems.AddItems)),
            transpiler: new HarmonyMethod(LevelRandomItems_AddItems_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> LevelRandomItems_Collide_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(32)]);

        cursor.EmitDelegate((int x) =>
        {
            if (WiderSetModule.IsWide)
            {
                return x + 10;
            }

            return x;
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> LevelRandomItems_AddItems_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(16)]))
            {
                cursor.EmitDelegate((int x) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 5;
                    }
                    return x;
                });
            }
        });

        cursor.Index = 0;

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(15)]))
            {
                cursor.EmitDelegate((int x) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 5;
                    }
                    return x;
                });
            }
        });

        cursor.Index = 0;

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(32)]))
            {
                cursor.EmitDelegate((int x) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 10;
                    }
                    return x;
                });
            }
        });

        cursor.Index = 0;

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(31)]))
            {
                cursor.EmitDelegate((int x) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 10;
                    }
                    return x;
                });
            }
        });

        cursor.Index = 0;

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(30)]))
            {
                cursor.EmitDelegate((int x) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 10;
                    }
                    return x;
                });
            }
        });

        cursor.Index = 0;

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(29)]))
            {
                cursor.EmitDelegate((int x) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 10;
                    }
                    return x;
                });
            }
        });

        cursor.Index = 0;

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(17)]))
            {
                cursor.EmitDelegate((int x) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 5;
                    }
                    return x;
                });
            }
        });

        cursor.Index = 0;

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(11)]))
            {
                cursor.EmitDelegate((int x) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 5;
                    }
                    return x;
                });
            }
        });

        cursor.Index = 0;

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.Ldstr("310")]))
            {
                cursor.EmitDelegate((string x) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return (int.Parse(x) + 100).ToString();
                    }
                    return x;
                });
            }
        });

        cursor.Index = 0;

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.Ldstr("160")]))
            {
                cursor.EmitDelegate((string x) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return (int.Parse(x) + 50).ToString();
                    }
                    return x;
                });
            }
        });

        return cursor.Generate();
    }
}
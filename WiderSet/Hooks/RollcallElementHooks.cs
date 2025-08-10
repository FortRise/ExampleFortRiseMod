using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using FortRise.Transpiler;
using FortRise;
using TowerFall;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using Monocle;

namespace Teuria.WiderSet;

internal sealed class RollcallElementHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(RollcallElement), [typeof(int)]),
            transpiler: new HarmonyMethod(RollcallElement_ctor_Transpiler)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(RollcallElement), "NotJoinedUpdate"),
            transpiler: new HarmonyMethod(RollcallElement_NotJoinedUpdate_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(RollcallElement), "HandleControlIcons"),
            transpiler: new HarmonyMethod(RollcallElement_HandleControlIcons_Update_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(RollcallElement), nameof(RollcallElement.Update)),
            transpiler: new HarmonyMethod(RollcallElement_HandleControlIcons_Update_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(RollcallElement), nameof(RollcallElement.Render)),
            new HarmonyMethod(RollcallElement_Render_Prefix),
            transpiler: new HarmonyMethod(RollcallElement_Render_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredPropertyGetter(typeof(RollcallElement), "MaxPlayers"),
            transpiler: new HarmonyMethod(RollcallElement_get_MaxPlayers_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(RollcallElement), nameof(RollcallElement.GetPosition)),
            new HarmonyMethod(RollcallElement_GetPosition_Prefix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(RollcallElement), nameof(RollcallElement.GetTweenSource)),
            new HarmonyMethod(RollcallElement_GetTweenSource_Prefix)
        );
    }

    private static void RollcallElement_Render_Prefix(RollcallElement __instance)
    {
        if (WiderSetModule.IsWide)
        {
            var playerIndex = DynamicData.For(__instance).Get<int>("playerIndex");
            Draw.OutlineTextCentered(
                TFGame.Font, $"P{playerIndex + 1}",
                __instance.Position + new Vector2(15, -40),
                ArcherData.GetColorA(playerIndex), 2f
            );
        }
    }

    private static IEnumerable<CodeInstruction> RollcallElement_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        // if (input != null)
        if (!RiseCore.IsWindows)
        {
            cursor.GotoNext(
                MoveType.After,
                [
                    ILMatch.Ldnull(),
                    ILMatch.Ceq(),
                    ILMatch.Stloc(),
                    ILMatch.Ldloc()
                ]
            );

            // NOTE: The return type has to be inverted, the expected value should've been false to be true
            cursor.EmitDelegate((bool inputNotNull) =>
            {
                return inputNotNull || WiderSetModule.IsWide;
            });
        }
        else
        {
            // *we have a regression here with Windows where it does not have these 4 instructions 

            cursor.GotoNext(
                MoveType.After,
                [
                    ILMatch.Ldfld("input")
                ]
            );

            cursor.EmitDelegate((PlayerInput input) =>
            {
                return input != null && !WiderSetModule.IsWide;
            });
        }


        // if (... && GameData.DarkWorldDLC)
        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Call("get_DarkWorldDLC")
            ]
        );

        cursor.EmitDelegate((bool dlcRequired) =>
        {
            return dlcRequired && !WiderSetModule.IsWide;
        });


        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> RollcallElement_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcR4(60)
            ]
        );

        cursor.EmitDelegate((float x) =>
        {
            if (WiderSetModule.IsWide)
            {
                return x - 30;
            }
            return x;
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> RollcallElement_HandleControlIcons_Update_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Ldsfld("ControlIconPos")
            ]
        );

        cursor.EmitDelegate((Vector2 pos) =>
        {
            if (WiderSetModule.IsWide)
            {
                return pos - new Vector2(15, 120);
            }

            return pos;
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> RollcallElement_NotJoinedUpdate_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(4)]);
        cursor.EmitDelegate((int player) => player + 4);

        cursor.GotoNext(MoveType.After, [
            ILMatch.Call("get_MainMenu"),
            ILMatch.LdcI4(3)
        ]);

        cursor.EmitDelegate((MainMenu.MenuState state) => 
        {
            if (MainMenu.RollcallMode == MainMenu.RollcallModes.Versus)
            {
                return WiderSetModule.StandardSelectionEntry.MenuState;
            }

            return state;
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> RollcallElement_get_MaxPlayers_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
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

        cursor.Index = 0;

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcI4(2)
            ]
        );

        cursor.EmitDelegate((int playerAmount) =>
        {
            if (WiderSetModule.IsWide)
            {
                return playerAmount + 2;
            }

            return playerAmount;
        });

        return cursor.Generate();
    }

    private static bool RollcallElement_GetPosition_Prefix(int playerIndex, ref Vector2 __result)
    {
        if (WiderSetModule.IsWide)
        {
            float num = 10 + playerIndex % 4 * 70;
            float numY = 75;
            if (playerIndex >= 4)
            {
                numY = 165f;
            }
            __result = new Vector2(45 + num, numY);
            return false;
        }
        return true;
    }

    private static bool RollcallElement_GetTweenSource_Prefix(int playerIndex, ref Vector2 __result)
    {
        if (WiderSetModule.IsWide)
        {
            Vector2 position = RollcallElement.GetPosition(playerIndex);
            if (playerIndex < 4)
            {
                __result = position + Vector2.UnitX * -420f;
            }
            else
            {
                __result = position + Vector2.UnitX * 420f;
            }

            return false;
        }
        return true;
    }
}

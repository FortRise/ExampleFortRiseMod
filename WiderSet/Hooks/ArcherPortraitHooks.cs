using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class ArcherPortraitHooks : IHookable
{
    private static FastReflectionHelper.FastInvoker FlipSide = null!;

    public static void Load(IHarmony harmony)
    {
        FlipSide = FastReflectionHelper.GetFastInvoker(
            AccessTools.DeclaredPropertyGetter(typeof(ArcherPortrait), "FlipSide")
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(ArcherPortrait), "SetCharacter"),
            transpiler: new HarmonyMethod(ArcherPortrait_SetCharacter_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(ArcherPortrait), nameof(ArcherPortrait.StartJoined)),
            transpiler: new HarmonyMethod(ArcherPortrait_StartJoined_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(ArcherPortrait), "Leave"),
            transpiler: new HarmonyMethod(ArcherPortrait_Leave_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(ArcherPortrait), "InitGem"),
            transpiler: new HarmonyMethod(ArcherPortrait_InitGem_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(ArcherPortrait), [typeof(Vector2), typeof(int), typeof(ArcherData.ArcherTypes), typeof(bool)]),
            transpiler: new HarmonyMethod(ArcherPortrait_ctor_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(ArcherPortrait), nameof(ArcherPortrait.Render)),
            new HarmonyMethod(ArcherPortrait_Render_Prefix)
        );
    }

    private static void ArcherPortrait_Render_Prefix(ArcherPortrait __instance)
    {
        if (WiderSetModule.IsWide)
        {
            __instance.ShowTitle = false;
        }
    }

    private static IEnumerable<CodeInstruction> ArcherPortrait_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);
        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.Initobj<Rectangle?>(), ILMatch.Ldloc()]))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate((Rectangle? rect, ArcherPortrait self) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        ref var offset = ref CollectionsMarshal.GetValueRefOrNullRef(WiderSetModule.NotJoinedCharacterOffset, self.CharacterIndex);
                        if (Unsafe.IsNullRef(ref offset))
                        {
                            return new Rectangle(0, 10, 60, 60);
                        }
                        return new Rectangle((int)offset.X, 10 + (int)offset.Y, 60, 60);
                    }
                    return rect;
                });
            }
        });

        cursor.Index = 0;

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcI4(120)
            ]
        );

        cursor.EmitDelegate((int x) =>
        {
            if (WiderSetModule.IsWide)
            {
                return x - 60;
            }

            return x;
        });


        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> ArcherPortrait_SetCharacter_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.Initobj<Rectangle?>(), ILMatch.Ldloc()]);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((Rectangle? rect, ArcherPortrait self) =>
        {
            if (WiderSetModule.IsWide)
            {
                var dict = self.AltSelect == ArcherData.ArcherTypes.Alt ?
                    WiderSetModule.NotJoinedAltCharacterOffset :
                    WiderSetModule.NotJoinedCharacterOffset;
                ref var offset = ref CollectionsMarshal.GetValueRefOrNullRef(dict, self.CharacterIndex);
                Rectangle rec;
                if (Unsafe.IsNullRef(ref offset))
                {
                    rec = new Rectangle(0, 10, 60, 60);
                }
                else
                {
                    rec = new Rectangle((int)offset.X, 10 + (int)offset.Y, 60, 60);
                }

                return self.ArcherData.Portraits.NotJoined.GetAbsoluteClipRect(rec);
            }
            return rect;
        });

        cursor.GotoNext(MoveType.After, [ILMatch.Initobj<Rectangle?>(), ILMatch.Ldloc()]);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((Rectangle? rect, ArcherPortrait self) =>
        {
            if (WiderSetModule.IsWide)
            {
                ref var offset = ref CollectionsMarshal.GetValueRefOrNullRef(WiderSetModule.NotJoinedAltCharacterOffset, self.CharacterIndex);
                Rectangle rec;
                if (Unsafe.IsNullRef(ref offset))
                {
                    rec = new Rectangle(0, 10, 60, 60);
                }
                else
                {
                    rec = new Rectangle((int)offset.X, 10 + (int)offset.Y, 60, 60);
                }

                return ((ArcherData)FlipSide.Invoke(self)!).Portraits.NotJoined.GetAbsoluteClipRect(rec);
            }
            return rect;
        });

        cursor.LogInstructions();

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> ArcherPortrait_StartJoined_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);
        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Initobj<Rectangle?>(),
                ILMatch.Ldloc()
            ]
        );

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((Rectangle? rect, ArcherPortrait self) =>
        {
            if (WiderSetModule.IsWide)
            {
                return self.ArcherData.Portraits.Joined.GetAbsoluteClipRect(new Rectangle(0, 10, 60, 60));
            }
            return rect;
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> ArcherPortrait_Leave_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);
        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Initobj<Rectangle?>(),
                ILMatch.Ldloc()
            ]
        );

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((Rectangle? rect, ArcherPortrait self) =>
        {
            if (WiderSetModule.IsWide)
            {
                ref var offset = ref CollectionsMarshal.GetValueRefOrNullRef(WiderSetModule.NotJoinedCharacterOffset, self.CharacterIndex);
                Rectangle rec;
                if (Unsafe.IsNullRef(ref offset))
                {
                    rec = new Rectangle(0, 10, 60, 60);
                }
                else
                {
                    rec = new Rectangle((int)offset.X, 10 + (int)offset.Y, 60, 60);
                }

                return self.ArcherData.Portraits.NotJoined.GetAbsoluteClipRect(rec);
            }
            return rect;
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> ArcherPortrait_InitGem_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
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
}
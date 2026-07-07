using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.Profiles;

[HarmonyPatch(typeof(RollcallElement))]
internal static class RollcallElementHooks
{
    [HarmonyPatch(MethodType.Constructor, [typeof(int)])]
    [HarmonyPrefix]
    public static void RollcallElementConstructor_Prefix(RollcallElement __instance)
    {
        DynamicData.For(__instance).Set("Teuria.Profiles/selection", 0);
        DynamicData.For(__instance).Set("Teuria.Profiles/ease", 0f);
    }

    [HarmonyPatch("ChangeSelectionRight")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ChangeSelectionRight_Prefix(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(1), ILMatch.Match(OpCodes.Add)]);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((int subbed, RollcallElement __instance) =>
        {
            var playerIndex = Private.Field<RollcallElement, int>("playerIndex", __instance).Read();
            var profileActive = ProfilesModule.Instance.ProfileActive[playerIndex];           
            var archerTypePtr = Private.Field<RollcallElement, ArcherData.ArcherTypes>("archerType", __instance);
            if (profileActive is null or { SelectedArchers.Count: 0 })
            {
                return subbed;
            }
            var indexes = profileActive.SelectedArchers.Select(x => x.CharacterIndex).ToHashSet();
            var altIndexes = profileActive.SelectedArchers
                .Where(x => x.ArcherType == ArcherData.ArcherTypes.Alt)
                .Select(x => x.CharacterIndex)
                .ToHashSet();

            while (!indexes.Contains(subbed))
            {
                subbed = (subbed + 1) % ArcherData.Amount;
            }

            if (altIndexes.Contains(subbed))
            {
                archerTypePtr.Write(ArcherData.ArcherTypes.Alt);
            }
            else
            {
                archerTypePtr.Write(ArcherData.ArcherTypes.Normal);
            }

            return subbed;
        });

        cursor.GotoNext([ILMatch.Br_S()]);

        var label = cursor.CreateLabel();
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((RollcallElement __instance) =>
        {
            var playerIndex = Private.Field<RollcallElement, int>("playerIndex", __instance).Read();
            var profileActive = ProfilesModule.Instance.ProfileActive[playerIndex];
            
            return profileActive is not null;
        });
        cursor.Emit(OpCodes.Brtrue_S, label);

        cursor.GotoNext(MoveType.After, [ILMatch.CallOrCallvirt("IsArcherBlacklisted"), ILMatch.Brtrue_S()]);
        cursor.GotoPrev();
        cursor.MarkLabel(label);

        return cursor.Generate();
    }

    [HarmonyPatch("ChangeSelectionLeft")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ChangeSelectionLeft_Prefix(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(1), ILMatch.Match(OpCodes.Sub)]);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((int subbed, RollcallElement __instance) =>
        {
            var playerIndex = Private.Field<RollcallElement, int>("playerIndex", __instance).Read();
            var archerTypePtr = Private.Field<RollcallElement, ArcherData.ArcherTypes>("archerType", __instance);
            var profileActive = ProfilesModule.Instance.ProfileActive[playerIndex];           
            if (profileActive is null or { SelectedArchers.Count: 0 })
            {
                return subbed;
            }
            var indexes = profileActive.SelectedArchers.Select(x => x.CharacterIndex).ToHashSet();
            var altIndexes = profileActive.SelectedArchers
                .Where(x => x.ArcherType == ArcherData.ArcherTypes.Alt)
                .Select(x => x.CharacterIndex)
                .ToHashSet();

            while (!indexes.Contains(subbed))
            {
                subbed = (subbed - 1) % ArcherData.Amount;
                if (subbed < 0)
                {
                    subbed = ArcherData.Amount;
                }
            }

            if (altIndexes.Contains(subbed))
            {
                archerTypePtr.Write(ArcherData.ArcherTypes.Alt);
            }
            else
            {
                archerTypePtr.Write(ArcherData.ArcherTypes.Normal);
            }

            return subbed;
        });

        cursor.GotoNext([ILMatch.Br_S()]);

        var label = cursor.CreateLabel();
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((RollcallElement __instance) =>
        {
            var playerIndex = Private.Field<RollcallElement, int>("playerIndex", __instance).Read();
            var profileActive = ProfilesModule.Instance.ProfileActive[playerIndex];
            
            return profileActive is not null;
        });
        cursor.Emit(OpCodes.Brtrue_S, label);

        cursor.GotoNext(MoveType.After, [ILMatch.CallOrCallvirt("IsArcherBlacklisted"), ILMatch.Brtrue_S()]);
        cursor.GotoPrev();
        cursor.MarkLabel(label);

        return cursor.Generate();
    }

    [HarmonyPatch("EnforceCharacterLock")]
    [HarmonyPrefix]
    public static bool EnforceCharacterLock_Prefix(RollcallElement __instance)
    {
        var playerIndex = Private.Field<RollcallElement, int>("playerIndex", __instance).Read();
        return ProfilesModule.Instance.ProfileActive[playerIndex] is null;
    }

    [HarmonyPatch("NotJoinedUpdate")]
    [HarmonyPrefix]
    public static bool NotJoinedUpdate_Prefix(RollcallElement __instance)
    {
        var playerIndex = Private.Field<RollcallElement, int>("playerIndex", __instance).Read();
        if (ProfilesModule.Instance.RollcallProfileActive[playerIndex])
        {
            return false;
        }

        var input = Private.Field<RollcallElement, PlayerInput>("input", __instance).Read();
        if (input is not null && input.MenuConfirmOrStart && TFGame.CharacterTaken(__instance.CharacterIndex))
        {
            var portrait = Private.Field<RollcallElement, ArcherPortrait>("portrait", __instance).Read();
            Private.Field<RollcallElement, float>("shakeTimer", __instance).Write(30);
            Sounds.ui_invalid.Play(__instance.X, 1f);
            input.Rumble(1f, 20);
            portrait.Shake();
        }


        return true;
    }

    [HarmonyPatch("NotJoinedUpdate")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> NotJoinedUpdate_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.Ldfld("input"), ILMatch.CallOrCallvirt("get_MenuAlt"), ILMatch.Brfalse_S()]);
        var blocks = cursor.Prev.operand;
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((RollcallElement __instance) =>
        {
            var playerIndex = Private.Field<RollcallElement, int>("playerIndex", __instance).Read();
            var profile = ProfilesModule.Instance.ProfileActive[playerIndex];
            
            return profile is null;
        });
        cursor.Emit(OpCodes.Brfalse_S, blocks);

        return cursor.Generate();
    }

    [HarmonyPatch("NotJoinedUpdate")]
    [HarmonyPostfix]
    public static void NotJoinedUpdate_Postfix(RollcallElement __instance, in int __result)
    {
        if (__result != 0)
        {
            return;
        }

        var playerIndex = Private.Field<RollcallElement, int>("playerIndex", __instance).Read();

        var portrait = Private.Field<RollcallElement, ArcherPortrait>("portrait", __instance).Read();
        var input = Private.Field<RollcallElement, PlayerInput>("input", __instance).Read();
        if (input is null)
        {
            return;
        }

        if (input.MenuArrows)
        {
            if (ProfilesModule.Instance.EnabledProfile.Count == 0)
            {
                Private.Field<RollcallElement, float>("shakeTimer", __instance).Write(30);
                Sounds.ui_invalid.Play(__instance.X, 1f);
                input.Rumble(1f, 20);
                portrait.Shake();
                return;
            }
            ProfilesModule.Instance.RollcallProfileActive[playerIndex] = true;
            portrait.ShowTitle = false;
            Sounds.ui_click.Play();
            return;
        }

        if (ProfilesModule.Instance.RollcallProfileActive[playerIndex])
        {
            var selection = DynamicData.For(__instance).Get<int>("Teuria.Profiles/selection");
            var ease = DynamicData.For(__instance).Get<float>("Teuria.Profiles/ease");
            ease = MathHelper.Lerp(ease, selection, Ease.SineIn(Engine.TimeMult * 0.8f));
            DynamicData.For(__instance).Set("Teuria.Profiles/ease", ease);

            if (MenuInput.Up)
            {
                DynamicData.For(__instance).Set("Teuria.Profiles/selection", (int)MathHelper.Clamp(selection - 1, 0, ProfilesModule.Instance.EnabledProfile.Count - 1));
            }

            if (MenuInput.Down)
            {
                DynamicData.For(__instance).Set("Teuria.Profiles/selection", (int)MathHelper.Clamp(selection + 1, 0, ProfilesModule.Instance.EnabledProfile.Count - 1));
            }

            if (MenuInput.Confirm)
            {
                var profile = ProfilesModule.Instance.EnabledProfile[selection];
                var actProfile = ProfilesModule.Instance.ProfileActive[playerIndex];
                if (actProfile is not null && profile.Name == actProfile.Name)
                {
                    ProfilesModule.Instance.ProfileActive[playerIndex] = null;
                    Close();
                    return;
                }

                if (profile.SelectedArchers.Count > 0)
                {
                    ChangeSelection(__instance, profile.SelectedArchers[0].ArcherID, profile.SelectedArchers[0].ArcherType);
                }
                if (input is XGamepadInput xGamepadInput)
                {
                    xGamepadInput.Config = profile.GamepadConfig;
                    xGamepadInput.RefreshButton();
                }
                else if (!profile.FollowsDefaultKeyboardConfig && input is KeyboardInput keyboardInput)
                {
                    keyboardInput.Config = profile.KeyboardConfig;
                }

                ProfilesModule.Instance.ProfileActive[playerIndex] = profile;

                Close();
                return;
            }

            if (MenuInput.Back)
            {
                Close();
            }


            void Close()
            {
                Sounds.ui_clickBack.Play();
                portrait.ShowTitle = true;
                ProfilesModule.Instance.RollcallProfileActive[playerIndex] = false;
            }
        }
    }

    private static void ChangeSelection(RollcallElement __instance, string archerDataName, ArcherData.ArcherTypes archerType)
    {
        ArcherData.ArcherTypes archerTypes = archerType;
        ArcherData? archerData;

        var entry = ProfilesModule.Instance.Context.Registry.Archers.RegisteredArchers
            .Select(x => x.Value)
            .FirstOrDefault(x => x.Name == archerDataName);

        if (entry is null)
        {
            archerData = ArcherData.Archers.Union(ArcherData.AltArchers)
                .Union(ArcherData.SecretArchers)
                .Where(x => x is not null)
                .FirstOrDefault(x => x.Name0 + x.Name1 == archerDataName);

            var index = ArcherData.Archers.IndexOf(archerData);
            if (index == -1)
            {
                index = ArcherData.AltArchers.IndexOf(archerData);
                if (index == -1)
                {
                    index = ArcherData.SecretArchers.IndexOf(archerData);
                }
            }

            __instance.CharacterIndex = index;
        }
        else
        {
            archerData = entry.ArcherData;

            var index = ArcherData.Archers.IndexOf(archerData);
            if (index == -1)
            {
                index = ArcherData.AltArchers.IndexOf(archerData);
                archerTypes = ArcherData.ArcherTypes.Alt;
                if (index == -1)
                {
                    index = ArcherData.SecretArchers.IndexOf(archerData);
                    archerTypes = ArcherData.ArcherTypes.Secret;
                }
            }

            __instance.CharacterIndex = index;
        }


        var portrait = Private.Field<RollcallElement, ArcherPortrait>("portrait", __instance).Read();
        Private.Field<RollcallElement, ArcherData.ArcherTypes>("archerType", __instance).Write(archerTypes);

        var rightArrow = Private.Field<RollcallElement, Image>("rightArrow", __instance).Read();
        var leftArrow = Private.Field<RollcallElement, Image>("leftArrow", __instance).Read();

        var playerIndex = Private.Field<RollcallElement, int>("playerIndex", __instance).Read();


        portrait.SetCharacter(__instance.CharacterIndex, archerTypes, 1);
        GamePad.SetLightBarEXT((PlayerIndex)playerIndex, portrait.ArcherData.LightbarColor);
        rightArrow.Color = leftArrow.Color = Color.Lerp(ArcherData.Archers[__instance.CharacterIndex].ColorB, Color.White, 0.5f);
    }



    [HarmonyPatch("Render")]
    [HarmonyPostfix]
    public static void Render_Postfix(RollcallElement __instance)
    {
        var input = Private.Field<RollcallElement, PlayerInput>("input", __instance).Read();
        var shakeOffset = Private.Field<RollcallElement, Vector2>("shakeOffset", __instance).Read();
        var playerIndex = Private.Field<RollcallElement, int>("playerIndex", __instance).Read();

        if (input != null)
        {
            var profile = ProfilesModule.Instance.ProfileActive[playerIndex];
            var text = profile is {} ? profile.Name.ToUpperInvariant() : "PROFILE";
            Color color = ArcherData.Archers[__instance.CharacterIndex].ColorA;
            var pos = new Vector2(-11f, -70);

            if (ProfilesModule.Instance.IWiderSetModAPI is { IsWide: true })
            {
                pos += new Vector2(-18f, 30);
                Draw.OutlineTextureJustify(input.ArrowsIcon, __instance.Position + pos + shakeOffset, new Vector2(1.1f, 0));
                var offsetY = new Vector2(-8, 6);
                for (int i = 0; i < text.Length; i += 1)
                {
                    Draw.OutlineTextJustify(TFGame.Font, text[i].ToString(), __instance.Position + pos + offsetY + shakeOffset, color, Color.Black, new Vector2(0, -1));
                    offsetY.Y += 6;
                }
            }
            else
            {
                Draw.OutlineTextureJustify(input.ArrowsIcon, __instance.Position + pos + shakeOffset, new Vector2(1.1f, 0));
                Draw.OutlineTextJustify(TFGame.Font, text, __instance.Position + pos + shakeOffset, color, Color.Black, new Vector2(0, -1));
            }

            var state = Private.Field<RollcallElement, StateMachine>("state", __instance).Read();

            if (profile is not null && state == 0 && TFGame.CharacterTaken(__instance.CharacterIndex))
            {
                Draw.OutlineTextureCentered(ProfilesModule.Instance.SingleLock, __instance.Position + shakeOffset, Color.White);
            }
        }

        if (!ProfilesModule.Instance.RollcallProfileActive[playerIndex])
        {
            return;
        }
        var actProfile = ProfilesModule.Instance.ProfileActive[playerIndex];

        int selection = DynamicData.For(__instance).Get<int>("Teuria.Profiles/selection");
        float ease = DynamicData.For(__instance).Get<float>("Teuria.Profiles/ease");

        var activeProfile = ProfilesModule.Instance.EnabledProfile;

        foreach (var mockup in activeProfile)
        {
            float index = activeProfile.IndexOf(mockup);
            int maxItem = ProfilesModule.Instance.IWiderSetModAPI is { IsWide: true } ? 2 : 4;

            float alpha;
            Vector2 posY = -(Vector2.UnitY * (ease - index) * 10);
            if (index < selection)
            {
                alpha = Math.Max(0f, (1f - ease + index + maxItem) / 1.5f);
            }
            else
            {
                alpha = Math.Max(0f, (1f + ease - index + maxItem) / 1.5f);
            }

            Color color = Color.White;

            if (actProfile?.Name == mockup.Name)
            {
                color = Color.Red;
            }
            else if (selection == index)
            {
                color = Color.Yellow;
            }

            Draw.OutlineTextCentered(TFGame.Font, mockup.Name.ToUpperInvariant(), __instance.Position + posY, color * alpha, Color.Black * alpha);
        }
    }

    [HarmonyPatch("Render")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.Before, [ILMatch.Ldarg(0), ILMatch.Ldfld("confirmButton"), ILMatch.Ldarg(0)]);

        var op = cursor.Prev.operand;

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((RollcallElement __instance) =>
        {
            var playerIndex = Private.Field<RollcallElement, int>("playerIndex", __instance).Read();
            return ProfilesModule.Instance.RollcallProfileActive[playerIndex] || TFGame.CharacterTaken(__instance.CharacterIndex);
        });

        cursor.Emit(OpCodes.Brtrue, op);

        cursor.GotoNext(MoveType.Before, [ILMatch.Ldarg(0), ILMatch.Ldfld("altFlash")]);

        var nextLabel = cursor.CreateLabel();

        cursor.Emit(new CodeInstruction(OpCodes.Ldarg_0));
        cursor.EmitDelegate((RollcallElement __instance) =>
        {
            var playerIndex = Private.Field<RollcallElement, int>("playerIndex", __instance).Read();

            var active = ProfilesModule.Instance.ProfileActive[playerIndex];

            if (active is null)
            {
                return ProfilesModule.Instance.RollcallProfileActive[playerIndex];
            }

            return active.SelectedArchers.Count > 0;
        });

        cursor.Emit(OpCodes.Brtrue, nextLabel);

        cursor.GotoNext([ILMatch.Ldarg(0), ILMatch.Ldfld("darkWorldLockEase")]);
        cursor.GotoPrev();
        cursor.MarkLabel(nextLabel);

        return cursor.Generate();
    }
}
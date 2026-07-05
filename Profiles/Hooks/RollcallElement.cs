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

    [HarmonyPatch("NotJoinedUpdate")]
    [HarmonyPrefix]
    public static bool NotJoinedUpdate_Prefix(RollcallElement __instance)
    {
        var playerIndex = Private.Field<RollcallElement, int>("playerIndex", __instance).Read();
        if (ProfilesModule.Instance.RollcallProfileActive[playerIndex])
        {
            return false;
        }
        return true;
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
                DynamicData.For(__instance).Set("Teuria.Profiles/selection", (int)MathHelper.Clamp(selection - 1, 0, ProfilesModule.Instance.Profiles.Count - 1));
            }

            if (MenuInput.Down)
            {
                DynamicData.For(__instance).Set("Teuria.Profiles/selection", (int)MathHelper.Clamp(selection + 1, 0, ProfilesModule.Instance.Profiles.Count - 1));
            }

            if (MenuInput.Confirm)
            {
                var profile = ProfilesModule.Instance.Profiles[selection];
                if (profile.ArcherID != null)
                {
                    ChangeSelection(__instance, profile.ArcherID, profile.ArcherTypes);
                }
                if (input is XGamepadInput xGamepadInput)
                {
                    xGamepadInput.Config = profile.GamepadConfig;
                    xGamepadInput.RefreshButton();
                }
                else if (input is KeyboardInput keyboardInput)
                {
                    keyboardInput.Config = profile.KeyboardConfig;
                }


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
        var controlIconPos = Traverse.Create(__instance).Field<Vector2>("ControlIconPos").Value;
        var input = Private.Field<RollcallElement, PlayerInput>("input", __instance).Read();
        var shakeOffset = Private.Field<RollcallElement, Vector2>("shakeOffset", __instance).Read();
        var playerIndex = Private.Field<RollcallElement, int>("playerIndex", __instance).Read();

        if (input != null)
        {
            Color color = ArcherData.Archers[__instance.CharacterIndex].ColorA;
            var pos = controlIconPos + Vector2.UnitY * 20f;
            Draw.OutlineTextureJustify(input.ArrowsIcon, __instance.Position + pos + shakeOffset, new Vector2(1.1f, 0));
            Draw.OutlineTextJustify(TFGame.Font, "PROFILE", __instance.Position + pos + shakeOffset, color, Color.Black, new Vector2(0, -1));
        }

        if (!ProfilesModule.Instance.RollcallProfileActive[playerIndex])
        {
            return;
        }

        int selection = DynamicData.For(__instance).Get<int>("Teuria.Profiles/selection");
        float ease = DynamicData.For(__instance).Get<float>("Teuria.Profiles/ease");

        foreach (var mockup in ProfilesModule.Instance.Profiles)
        {
            float index = ProfilesModule.Instance.Profiles.IndexOf(mockup);

            float alpha;
            Vector2 posY = -(Vector2.UnitY * (ease - index) * 10);
            if (index < selection)
            {
                alpha = Math.Max(0f, (1f - ease + index + 4) / 1.5f);
            }
            else
            {
                alpha = Math.Max(0f, (1f + ease - index + 4) / 1.5f);
            }

            Color color = Color.White;

            if (selection == index)
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
            return ProfilesModule.Instance.RollcallProfileActive[playerIndex];
        });

        cursor.Emit(OpCodes.Brtrue, op);

        cursor.GotoNext(MoveType.Before, [ILMatch.Ldarg(0), ILMatch.Ldfld("altFlash")]);

        var nextLabel = cursor.CreateLabel();

        cursor.Emit(new CodeInstruction(OpCodes.Ldarg_0));
        cursor.EmitDelegate((RollcallElement __instance) =>
        {
            var playerIndex = Private.Field<RollcallElement, int>("playerIndex", __instance).Read();
            return ProfilesModule.Instance.RollcallProfileActive[playerIndex];
        });

        cursor.Emit(OpCodes.Brtrue, nextLabel);

        cursor.GotoNext([ILMatch.Ldarg(0), ILMatch.Ldfld("darkWorldLockEase")]);
        cursor.GotoPrev();
        cursor.MarkLabel(nextLabel);

        return cursor.Generate();
    }
}
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using FortRise.Transpiler;
using HarmonyLib;
using Monocle;
using TowerFall;

namespace Teuria.Profiles;

[HarmonyPatch]
internal static class VersusRoundResultsHooks
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.EnumeratorMoveNext(AccessTools.DeclaredMethod(typeof(VersusRoundResults), "Sequence"));
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext([
            ILMatch.Ldloc().TryGetLocalIndex(out var pos),
            ILMatch.LdcI4(12),
            ILMatch.Match(OpCodes.Sub)
        ]); 


        cursor.GotoNext([
            ILMatch.Ldloc(), 
            ILMatch.CallOrCallvirt("get_Font"), 
            ILMatch.Ldloc(), 
            ILMatch.Ldloc(),
            ILMatch.Ldloc().TryGetLocalIndex(out var playerIndex),
            ILMatch.LdcI4(-1),
            ILMatch.CallOrCallvirt("GetColorA")
        ]);


        cursor.GotoNext(MoveType.After, [
            ILMatch.CallOrCallvirt("get_One"), 
            ILMatch.LdcR4(2), 
            ILMatch.CallOrCallvirt("op_Multiply"), 
            ILMatch.Stfld("Scale")
        ]);

        cursor.Emit(new CodeInstruction(OpCodes.Ldloca, playerIndex.Value));
        cursor.EmitDelegate((Text text, in int playerIndex) =>
        {
            if (ProfilesModule.Instance.GetSettings<ProfileSettings>()!.HideProfileNameOnRoundResult)
            {
                return text;
            }
            var profile = ProfilesModule.Instance.ProfileActive[playerIndex];
            if (profile is not null)
            {
                text.DrawText = profile.Name.ToUpperInvariant();
            }

            return text;
        });

        cursor.GotoNext(MoveType.After, [
            ILMatch.Ldloc(),
            ILMatch.Ldloc(),
            ILMatch.Ldfld("crowns"),
            ILMatch.Ldloc().TryGetLocalIndex(out var crownIndex),
            ILMatch.Match(OpCodes.Ldelem_Ref)
        ]);

        cursor.Emit(new CodeInstruction(OpCodes.Ldloca, crownIndex.Value));
        cursor.Emit(new CodeInstruction(OpCodes.Ldloca, pos.Value));
        cursor.EmitDelegate((Image image, in int crownIndex, in int textPosX) =>
        {
            if (ProfilesModule.Instance.GetSettings<ProfileSettings>()!.HideProfileNameOnRoundResult)
            {
                return image;
            }

            var profile = ProfilesModule.Instance.ProfileActive[crownIndex];
            if (profile is not null)
            {
                var textWidth = TFGame.Font.MeasureString(profile.Name.ToUpperInvariant()).X;
                image.Position.X = textPosX - textWidth - textWidth - image.Width - 13f;
            }

            return image;
        });

        return cursor.Generate();
    }
}
using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using TowerFall;

namespace Teuria.Profiles;

[HarmonyPatch(typeof(PlayerIndicator))]

internal static class PlayerIndicatorHooks
{
    [HarmonyPatch(MethodType.Constructor, [typeof(Vector2), typeof(int), typeof(bool)])]
    [HarmonyPostfix]
    public static void PlayerIndicatorConstructor_Postfix(PlayerIndicator __instance, int playerIndex)
    {
        if (ProfilesModule.Instance.GetSettings<ProfileSettings>()!.HideProfileNameOnPlayer)
        {
            return;
        }

        var profile = ProfilesModule.Instance.ProfileActive[playerIndex];
        if (profile is not null)
        {
            Private.Field<PlayerIndicator, string>("text", __instance).Write(profile.Name.ToUpperInvariant());
        }
    }
}

using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.Profiles;

[HarmonyPatch(typeof(VersusPlayerMatchResults))]
internal static class VersusPlayerMatchResultsHook
{
    [HarmonyPatch(nameof(VersusPlayerMatchResults.Render))]
    [HarmonyPostfix]
    public static void Render_Postfix(VersusPlayerMatchResults __instance)
    {
        var playerIndex = Private.Field<VersusPlayerMatchResults, int>("playerIndex", __instance).Read();
        var characterIndex = Private.Field<VersusPlayerMatchResults, int>("characterIndex", __instance).Read();

        var profile = ProfilesModule.Instance.ProfileActive[playerIndex];
        if (profile is not null)
        {
            var pos = Vector2.UnitY * 35;
            Color color = ArcherData.Archers[characterIndex].ColorA;
            Draw.OutlineTextCentered(
                TFGame.Font, 
                profile.Name.ToUpperInvariant(), 
                __instance.Position - pos, 
                color, 
                Color.Black
            );
        }
    }
}

using System.Collections.Generic;
using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using TowerFall;

namespace Teuria.QuestRandomizer;

[HarmonyPatch(typeof(Skeleton))]
internal class SkeletonHooks 
{
    [HarmonyPatch(nameof(Skeleton.Added))]
    [HarmonyPostfix]
    public static void Added_Postfix(Skeleton __instance)
    {
        bool jester = Private.Field<Skeleton, bool>("jester", __instance).Read();
        var warpPoints = Private.Field<Skeleton, List<Vector2>>("warpPoints", __instance).Read();
        if (jester)
        {
            var spawner = __instance.Level.GetXMLPositions("Spawner");
            warpPoints.InsertRange(0, spawner);
        }
    }
}


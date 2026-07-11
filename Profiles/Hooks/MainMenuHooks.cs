using System.Collections.Generic;
using System.Linq;
using FortRise;
using HarmonyLib;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.Profiles;

[HarmonyPatch(typeof(MainMenu))]
internal static class MainMenuHooks
{
    [HarmonyPatch(nameof(MainMenu.CreateMain))]
    [HarmonyPostfix]
    public static void CreateMain_Prefix()
    {
        for (int i = 0; i < ProfilesModule.Instance.ProfileActive.Length; i += 1)
        {
            ProfilesModule.Instance.DeactivateProfile(i);
        }
    }

    [HarmonyPatch(nameof(MainMenu.CreateArchives))]
    [HarmonyPostfix]
    public static void CreateArchives_Postfix(MainMenu __instance)
    {
        // the entity has not been added yet, it will be placed on toAdd.
        // let's catch it early
        var layer = __instance.Layers[-1];
        var toAdd = Private.Field<Layer, List<Entity>>("toAdd", layer).Read();

        ArchivesController? controller = null;

        foreach (var e in toAdd)
        {
            if (e is ArchivesController c)
            {
                controller = c;
            }
        }

        if (controller is null)
        {
            return;
        }

        var pagesPtr = Private.Field<ArchivesController, ArchivesPage[]>("pages", controller);
        ArchivesProfileSessionPage sessionPage;

        var pages = pagesPtr.Read().ToList();
        pages.Insert(2, sessionPage = new ArchivesProfileSessionPage());
        pagesPtr.Write([.. pages]);

        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].Controller = controller;
            pages[i].X = DynamicData.For(controller).Invoke<float>("GetPageX", i);
        }

        __instance.Add(sessionPage);
    }
}

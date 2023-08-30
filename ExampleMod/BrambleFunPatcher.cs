using System.Reflection;
using FortRise;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using TowerFall;

namespace ExampleMod;

public static class BrambleFunPatcher 
{ 
    private static IDetour hook_InvokeQuestSpawnPortal_FinishSpawn;

    internal static void Load() 
    {
        hook_InvokeQuestSpawnPortal_FinishSpawn = new ILHook(
            typeof(RiseCore.Events).GetMethod("InvokeQuestSpawnPortal_FinishSpawn", BindingFlags.NonPublic | BindingFlags.Static),
            InvokeFinishSpawn_patch            
        );

    }
    internal static void InvokeFinishSpawn_patch(ILContext ctx) 
    {
        var cursor = new ILCursor(ctx);
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdarg(2), instr => instr.MatchLdloc(2))) 
        {
            cursor.EmitDelegate((ArrowTypes arrowTypes) => 
            {
                if (ExampleModModule.Settings.AllTriggerBrambleArrow) 
                {
                    var brambleArrow = RiseCore.ArrowsRegistry["TriggerBrambleArrows"];
                    return brambleArrow.Types;
                }
                return arrowTypes;
            });
        }
    }

    internal static void Unload() 
    {
        hook_InvokeQuestSpawnPortal_FinishSpawn.Dispose();
    }
}
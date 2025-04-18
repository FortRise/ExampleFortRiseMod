using FortRise;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class ClumsySwap : IHookable
{
    public static void Load()
    {
        IL.TowerFall.Player.Update += Update_patch;
    }

    public static void Unload()
    {
        IL.TowerFall.Player.Update -= Update_patch;
    }

    private static void Update_patch(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);

        if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdsfld("TowerFall.Sounds", "sfx_arrowToggle")))
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((Player player) => {
                if (Variants.ClumsySwap.IsActive(player.PlayerIndex))
                {
                    DynamicData.For(player).Invoke("DropArrow");
                }
            });
        }
    }
}
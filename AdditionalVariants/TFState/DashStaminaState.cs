using MonoMod.Utils;
using System.Collections.Generic;
using System.Linq;
using TowerFall;

namespace Teuria.AdditionalVariants;

internal static class DashStaminaState
{
    public const string Name = "DashStamina";

    public static byte[] OnSaveState()
    {
        var level = TfStateInterop.CurrentLevel;
        if (level is null)
        {
            return [];
        }

        var players = level.Players
            .OfType<Player>()
            .OrderBy(player => player.PlayerIndex)
            .ToList();

        if (players.Count == 0)
        {
            return [];
        }

        return StateBuffer.Save(writer =>
        {
            writer.Write(players.Count);

            foreach (var player in players)
            {
                var data = DynamicData.For(player);
                var haveStamina = data.TryGetValue<DashStamina>("dashStamina", out var stamina);

                writer.Write(player.PlayerIndex);
                writer.Write(haveStamina);

                if (!haveStamina)
                {
                    continue;
                }

                writer.Write(stamina.Bar);
                writer.Write(stamina.Alpha);
            }
        });
    }

    public static void OnLoadState(byte[] state)
    {
        var level = TfStateInterop.CurrentLevel;
        if (level is null)
        {
            return;
        }

        var saved = new Dictionary<int, (bool HaveStamina, float Bar, float Alpha)>();

        StateBuffer.Load(state, reader =>
        {
            var playerCount = reader.ReadInt32();

            for (int i = 0; i < playerCount; i++)
            {
                var playerIndex = reader.ReadInt32();

                if (!reader.ReadBoolean())
                {
                    saved[playerIndex] = (false, 0f, 0f);
                    continue;
                }

                saved[playerIndex] = (true, reader.ReadSingle(), reader.ReadSingle());
            }
        });

        foreach (var player in level.Players.OfType<Player>())
        {
            if (!saved.TryGetValue(player.PlayerIndex, out var toLoad))
            {
                continue;
            }

            var data = DynamicData.For(player);
            data.TryGetValue<DashStamina>("dashStamina", out var stamina);

            if (!toLoad.HaveStamina)
            {
                if (stamina != null)
                {
                    stamina.RemoveSelf();
                    player.Components.Remove(stamina);
                    data.Set("dashStamina", null);
                }

                continue;
            }

            if (stamina == null)
            {
                stamina = new DashStamina(true, true);
                player.Add(stamina);
                data.Set("dashStamina", stamina);
            }

            stamina.Bar = toLoad.Bar;
            stamina.Alpha = toLoad.Alpha;
        }
    }
}

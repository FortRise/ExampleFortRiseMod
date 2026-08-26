using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System.Collections.Generic;
using System.Linq;
using TowerFall;

namespace Teuria.AdditionalVariants;

internal static class JesterHatState
{
    public const string Name = "JestersHat";

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
                var haveWarpPoints = data.TryGetValue<List<Vector2>>("warpPoints", out var warpPoints) && warpPoints.Count > 0;

                writer.Write(player.PlayerIndex);
                writer.Write(haveWarpPoints);

                if (!haveWarpPoints)
                {
                    continue;
                }

                data.TryGetValue<Vector2>("lastWarpPoint", out var lastWarpPoint);

                writer.Write(warpPoints.Count);

                foreach (var warpPoint in warpPoints)
                {
                    writer.WriteVector2(warpPoint);
                }

                writer.WriteVector2(lastWarpPoint);
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

        var saved = new Dictionary<int, (List<Vector2> WarpPoints, Vector2 LastWarpPoint)>();

        StateBuffer.Load(state, reader =>
        {
            var playerCount = reader.ReadInt32();

            for (int i = 0; i < playerCount; i++)
            {
                var playerIndex = reader.ReadInt32();

                if (!reader.ReadBoolean())
                {
                    saved[playerIndex] = ([], Vector2.Zero);
                    continue;
                }

                var warpPointCount = reader.ReadInt32();
                var warpPoints = new List<Vector2>(warpPointCount);

                for (int j = 0; j < warpPointCount; j++)
                {
                    warpPoints.Add(reader.ReadVector2());
                }

                saved[playerIndex] = (warpPoints, reader.ReadVector2());
            }
        });

        foreach (var player in level.Players.OfType<Player>())
        {
            if (!saved.TryGetValue(player.PlayerIndex, out var toLoad))
            {
                continue;
            }

            var data = DynamicData.For(player);
            data.Set("warpPoints", toLoad.WarpPoints);
            data.Set("lastWarpPoint", toLoad.LastWarpPoint);
        }
    }
}

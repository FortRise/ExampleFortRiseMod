using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using TowerFall;


namespace AdditionalVariants.EX.JesterHat
{
    public static class JesterHatStateEvents
    {
        public static void OnLoadState(string toLoad)
        {
            var state = JsonSerializer.Deserialize<JesterHatState>(toLoad)!;
            var players = (Monocle.Engine.Instance.Scene as Level)!.Players.Select(p => p as Player).ToList();
            foreach (Player player in players!)
            {
                var dynPlayer = DynamicData.For(player);
                if (state.JesterHatsWarpPoints.TryGetValue(player.PlayerIndex, out var warpPoints))
                {
                    var list = new List<Vector2>();
                    foreach (var item in warpPoints)
                    {
                        list.Add(new Vector2(item.X, item.Y));
                    }

                    dynPlayer.Set("warpPoints", list);
                }
            }
        }

        public static string OnSaveState()
        {
            var state = new JesterHatState();

            var players = (Monocle.Engine.Instance.Scene as Level)!.Players.Select(p => p as Player).ToList();

            ICollection<(int, IEnumerable<Vector2f>)> jesterHatWarpPoints = new List<(int, IEnumerable<Vector2f>)>();

            foreach (Player player in players!)
            {
                var dynPlayer = DynamicData.For(player);
                if (dynPlayer.TryGet<List<Vector2>>("warpPoints", out var warpPoints))
                {
                    var list = new List<Vector2f>();
                    foreach (var item in warpPoints)
                    {
                        list.Add(new Vector2f
                        {
                            X = item.X,
                            Y = item.Y
                        });
                    }
                    jesterHatWarpPoints.Add((player.PlayerIndex, list));
                }
            }
            state.JesterHatsWarpPoints = jesterHatWarpPoints.ToDictionary(x => x.Item1, x => x.Item2);

            var serialized = JsonSerializer.Serialize(state);

            return serialized.ToString();
        }
    }
}

using TowerFall;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Teuria.AdditionalVariants;

internal partial class ApiImplementation
{
    internal class JesterHatImplementation : IAdditionalVariantsAPI.IJesterHatAPI
    {
        public void RegisterHook(IAdditionalVariantsAPI.IJesterHatAPI.IHook hook, int priority = 0)
        {
            JesterHatManager.Instance.AddHook(hook, priority);
        }

        public void UnregisterHook(IAdditionalVariantsAPI.IJesterHatAPI.IHook hook)
        {
            JesterHatManager.Instance.RemoveHook(hook);
        }

        internal class ModifyWarpPointsArgs : IAdditionalVariantsAPI.IJesterHatAPI.IHook.IModifyWarpPointsArgs
        {
            public ModifyWarpPointsArgs(Player player, IList<Vector2> warpPoints)
            {
                Player = player;
                WarpPoints = warpPoints;
            }

            public Player Player { get; }
            public IList<Vector2> WarpPoints { get; }
        }

        internal class AfterTeleportArgs : IAdditionalVariantsAPI.IJesterHatAPI.IHook.IAfterTeleportArgs
        {
            public AfterTeleportArgs(Player player, Vector2 lastWarpPoint)
            {
                Player = player;
                LastWarpPoint = lastWarpPoint;
            }

            public Player Player { get; }
            public Vector2 LastWarpPoint { get; }
        }
    }
}

internal sealed class JesterHatManager 
{
    internal static readonly JesterHatManager Instance = new();

    internal OrderedList<IAdditionalVariantsAPI.IJesterHatAPI.IHook> Hooks { get; set; } = new();

    private JesterHatManager()
    {
    }

    public void AddHook(IAdditionalVariantsAPI.IJesterHatAPI.IHook hook, int priority = 0)
    {
        Hooks.Add(hook, priority);
    }

    public void RemoveHook(IAdditionalVariantsAPI.IJesterHatAPI.IHook hook)
    {
        Hooks.Remove(hook);
    }
}

internal sealed class OrderedList<T>(bool asecending = false) : IReadOnlyList<T>
{
    public record struct Entry(T Value, int Priority);

    private readonly List<Entry> entries = new List<Entry>();

    public T this[int index] => entries[index].Value;

    public int Count => entries.Count;

    public bool Ascending { get; } = asecending;

    public void Add(T element, int priority)
    {
        if (Ascending)
        {
            for (int i = 0; i < entries.Count; i += 1)
            {
                if (entries[i].Priority.CompareTo(priority) > 0)
                {
                    entries.Insert(i, new Entry(element, priority));
                    return;
                }
            }
        }
        else 
        {
            for (int i = 0; i < entries.Count; i += 1)
            {
                if (entries[i].Priority.CompareTo(priority) < 0)
                {
                    entries.Insert(i, new Entry(element, priority));
                    return;
                }
            }
        }

        entries.Add(new Entry(element, priority));
    }

    public bool Remove(T element)
    {
		for (var i = 0; i < entries.Count; i++)
		{
			if (Equals(entries[i].Value, element))
			{
				entries.RemoveAt(i);
				return true;
			}
		}
		return false;
    }

    public void Clear()
    {
        entries.Clear();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return entries.Select(x => x.Value).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
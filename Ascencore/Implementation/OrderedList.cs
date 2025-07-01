using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Teuria.Ascencore;

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
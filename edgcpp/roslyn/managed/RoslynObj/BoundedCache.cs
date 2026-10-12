namespace RoslynObj;

// Access is serialized by RoslynObject's snapshot lock.
internal sealed class BoundedCache<TKey, TValue>(int capacity) where TKey : notnull
{
    private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> entries = [];
    private readonly LinkedList<(TKey Key, TValue Value)> order = [];
    internal int Count => entries.Count;

    internal bool TryGet(TKey key, out TValue value)
    {
        if (!entries.TryGetValue(key, out var node))
        {
            value = default!;
            return false;
        }
        order.Remove(node);
        order.AddFirst(node);
        value = node.Value.Value;
        return true;
    }

    internal void Set(TKey key, TValue value)
    {
        if (entries.Remove(key, out var previous)) order.Remove(previous);
        entries.Add(key, order.AddFirst((key, value)));
        if (entries.Count <= capacity) return;
        var last = order.Last!;
        entries.Remove(last.Value.Key);
        order.RemoveLast();
    }

    internal void Clear()
    {
        entries.Clear();
        order.Clear();
    }
}

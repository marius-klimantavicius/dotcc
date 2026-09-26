using static global::Managed.Database.Sqlite;

internal static unsafe partial class Program
{
    private static void CheckCursorInitialization()
    {
        // Upstream intentionally clears only offsetof(BtCursor, pBt), leaving
        // the large trailing arrays for later initialization (btree.c comment).
        BtCursor cursor;
        var bytes = new Span<byte>(&cursor, sizeof(BtCursor));
        bytes.Fill(0xA5);
        int prefix = checked((int)((byte*)&cursor.pBt - (byte*)&cursor));
        sqlite3BtreeCursorZero(&cursor);
        if (prefix != 32 || bytes[..prefix].ContainsAnyExcept((byte)0)
            || bytes[prefix..].ContainsAnyExcept((byte)0xA5))
            throw new InvalidOperationException("BtCursor prefix initialization crossed the offsetof boundary");
        Console.WriteLine($"managed cursor: clears {prefix} of {sizeof(BtCursor)} bytes; trailing fields preserved");
    }
}

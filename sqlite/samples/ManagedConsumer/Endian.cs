using System.Text;
using Managed.Database;
using static global::Managed.Database.Sqlite;

internal static unsafe partial class Program
{
    private const string EndianText = "Aéλ水😀";

    private static int EndianEntry(string[] args)
    {
        try
        {
            if (args.Length != 3 || args[0] != "endian" || args[1] is not ("write" or "read"))
                throw new ArgumentException("Usage: ManagedConsumer endian write|read DIRECTORY");
            Directory.CreateDirectory(args[2]);
            CheckEndianDatabases(args[2], args[1] == "write");
            if (HostVfs.OpenHandleCount != 0 || sqlite3_shutdown() != SQLITE_OK)
                throw new InvalidOperationException("Endian oracle leaked SQLite resources");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    private static void CheckEndianDatabases(string directory, bool write)
    {
        foreach (var encoding in new[] { "UTF-16le", "UTF-16be" })
        {
            var path = Path.Combine(directory, encoding + ".db");
            if (write)
            {
                File.Delete(path);
                sqlite3* writer = null;
                fixed (char* name = path) Check(sqlite3_open16(name, &writer), writer, "UTF16 open");
                try
                {
                    Execute(writer, $"PRAGMA encoding='{encoding}';");
                    Expect(writer, "PRAGMA journal_mode=WAL", "wal");
                    Execute(writer, "CREATE TABLE endian_text(id INTEGER PRIMARY KEY, value TEXT);");
                    sqlite3_stmt* insert = null;
                    fixed (char* sql = "INSERT INTO endian_text VALUES(?1,?2)")
                        Check(sqlite3_prepare16_v2(writer, sql, -1, &insert, null), writer, "UTF16 prepare");
                    try
                    {
                        for (int mode = 0; mode < 3; ++mode)
                        {
                            Check(sqlite3_bind_int(insert, 1, mode), writer, "bind id");
                            if (mode == 0)
                            {
                                fixed (char* value = EndianText)
                                    Check(sqlite3_bind_text16(insert, 2, value, EndianText.Length * 2, Transient), writer, "bind native UTF16");
                            }
                            else
                            {
                                var bytes = (mode == 1 ? Encoding.Unicode : Encoding.BigEndianUnicode).GetBytes(EndianText);
                                fixed (byte* value = bytes)
                                    Check(sqlite3_bind_text64(insert, 2, value, (ulong)bytes.Length, Transient,
                                        (byte)(mode == 1 ? SQLITE_UTF16LE : SQLITE_UTF16BE)), writer, "bind explicit UTF16");
                            }
                            if (sqlite3_step(insert) != SQLITE_DONE) throw new InvalidOperationException("UTF16 insert failed");
                            Check(sqlite3_reset(insert), writer, "UTF16 reset");
                        }
                    }
                    finally { Check(sqlite3_finalize(insert), writer, "UTF16 finalize insert"); }
                    Check(sqlite3_wal_checkpoint_v2(writer, null, SQLITE_CHECKPOINT_TRUNCATE, null, null), writer, "UTF16 WAL checkpoint");
                }
                finally { Check(sqlite3_close(writer), writer, "UTF16 close writer"); }
            }
            sqlite3* db = null;
            fixed (char* name = path) Check(sqlite3_open16(name, &db), db, "UTF16 reopen");
            try
            {
                Expect(db, "PRAGMA encoding", encoding);
                Expect(db, "PRAGMA integrity_check", "ok");
                Expect(db, "SELECT count(*) FROM endian_text WHERE value='Aéλ水😀' AND length(value)=5", "3");
                sqlite3_stmt* query = null;
                fixed (char* sql = "SELECT value FROM endian_text ORDER BY id")
                    Check(sqlite3_prepare16_v2(db, sql, -1, &query, null), db, "UTF16 query");
                try
                {
                    for (int row = 0; row < 3; ++row)
                    {
                        if (sqlite3_step(query) != SQLITE_ROW) throw new InvalidOperationException("Missing UTF16 row");
                        if (Utf8(sqlite3_column_text(query, 0)) != EndianText) throw new InvalidOperationException("UTF8 conversion");
                        var native = sqlite3_column_text16(query, 0);
                        if (new string((char*)native, 0, sqlite3_column_bytes16(query, 0) / 2) != EndianText)
                            throw new InvalidOperationException("Native UTF16 conversion");
                        var value = sqlite3_column_value(query, 0);
                        var little = sqlite3_value_text16le(value);
                        CheckEndianBytes(little, sqlite3_value_bytes16(value), Encoding.Unicode);
                        var big = sqlite3_value_text16be(value);
                        CheckEndianBytes(big, sqlite3_value_bytes16(value), Encoding.BigEndianUnicode);
                    }
                    if (sqlite3_step(query) != SQLITE_DONE) throw new InvalidOperationException("Extra UTF16 row");
                }
                finally { Check(sqlite3_finalize(query), db, "UTF16 finalize query"); }
            }
            finally { Check(sqlite3_close(db), db, "UTF16 close reader"); }
            Console.WriteLine($"PASS {encoding}: native/LE/BE APIs, conversions, WAL checkpoint and reopen");
        }
    }

    private static void CheckEndianBytes(void* pointer, int length, Encoding encoding)
    {
        if (pointer == null || !new ReadOnlySpan<byte>(pointer, length).SequenceEqual(encoding.GetBytes(EndianText)))
            throw new InvalidOperationException("Explicit UTF16 byte order mismatch: " + encoding.WebName);
    }
}

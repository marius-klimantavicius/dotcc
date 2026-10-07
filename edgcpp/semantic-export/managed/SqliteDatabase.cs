using System.Buffers.Binary;
using System.Buffers.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Edg.Semantic;

/// <summary>The small, readonly SQLite ABI needed by the semantic reader.</summary>
internal sealed unsafe class SqliteDatabase : IDisposable
{
    private const string Library = "edg_semantic_sqlite3";
    private nint handle;

    static SqliteDatabase() => NativeLibrary.SetDllImportResolver(typeof(SqliteDatabase).Assembly, ResolveLibrary);

    private static nint ResolveLibrary(string name, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (name != Library) return 0;
        string[] candidates = OperatingSystem.IsWindows() ? ["sqlite3.dll", "winsqlite3.dll"] :
            OperatingSystem.IsMacOS() ? ["/usr/lib/libsqlite3.dylib", "libsqlite3.dylib"] : ["libsqlite3.so.0", "libsqlite3.so"];
        foreach (var candidate in candidates)
            if (NativeLibrary.TryLoad(candidate, out var loaded)) return loaded;
        throw new DllNotFoundException("The semantic SQLite reader requires the platform SQLite library (sqlite3).");
    }

    public SqliteDatabase(string path)
    {
        var filename = Encoding.UTF8.GetBytes(Path.GetFullPath(path) + '\0');
        nint opened = 0;
        int result;
        fixed (byte* text = filename) result = Native.sqlite3_open_v2(text, &opened, 1 /* SQLITE_OPEN_READONLY */, null);
        handle = opened;
        if (result != 0) { var message = Error(result); Dispose(); throw message; }
        try
        {
            Execute("PRAGMA query_only=ON");
            Execute("PRAGMA trusted_schema=OFF");
            Execute("PRAGMA cache_size=-8192");
            Execute("PRAGMA mmap_size=0");
            Execute("BEGIN"); // Hold one read snapshot across integrity validation and all lazy reads.
        }
        catch { Dispose(); throw; }
    }

    internal static string Quote(string identifier) => "\"" + identifier.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    internal InvalidDataException Error(int code) => new("SQLite semantic store: " + (handle == 0 ? "open failed" : Marshal.PtrToStringUTF8((nint)Native.sqlite3_errmsg(handle))) + " (" + code + ")");
    public SqliteStatement Prepare(string sql)
    {
        ObjectDisposedException.ThrowIf(handle == 0, this);
        var bytes = Encoding.UTF8.GetBytes(sql + '\0');
        nint statement = 0;
        fixed (byte* text = bytes)
        {
            var result = Native.sqlite3_prepare_v2(handle, text, bytes.Length - 1, &statement, null);
            if (result != 0) throw Error(result);
        }
        return new SqliteStatement(this, statement);
    }

    public void Execute(string sql) { using var statement = Prepare(sql); while (statement.Step()) { } }
    public long Scalar(string sql) { using var statement = Prepare(sql); if (!statement.Step()) throw new InvalidDataException("Missing SQLite scalar result"); return statement.Integer(0); }

    public string ContentHash(out Dictionary<string, (string Hash, long Length)> sourceHashes)
    {
        sourceHashes = new(StringComparer.Ordinal);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData("edg-semantic-content/1\n"u8);
        var tables = new List<(string Name, string Sql)>();
        using (var list = Prepare("SELECT name,sql FROM sqlite_master WHERE type='table' AND name<>'package_info' AND substr(name,1,7)<>'sqlite_' ORDER BY name"))
            while (list.Step()) tables.Add((list.Text(0), list.Text(1)));
        foreach (var table in tables)
        {
            hash.AppendData("T"u8); AppendFrame(hash, Encoding.UTF8.GetBytes(table.Name));
            hash.AppendData("S"u8); AppendFrame(hash, Encoding.UTF8.GetBytes(table.Sql));
            var keys = new SortedDictionary<long, string>();
            using (var info = Prepare("PRAGMA table_info(" + Quote(table.Name) + ")"))
                while (info.Step()) if (info.Integer(5) != 0) keys.Add(info.Integer(5), info.Text(1));
            if (keys.Count == 0) throw new InvalidDataException("Semantic content table lacks a primary key: " + table.Name);
            using var rows = Prepare("SELECT * FROM " + Quote(table.Name) + " ORDER BY " + string.Join(",", keys.Values.Select(Quote)));
            while (rows.Step())
            {
                hash.AppendData("R"u8);
                for (var column = 0; column < rows.ColumnCount; ++column) rows.HashCell(column, hash);
                hash.AppendData("\n"u8);
                if (table.Name == "source_blobs")
                {
                    if (rows.ColumnCount != 2 || rows.Type(0) != 3 || rows.Type(1) != 4) throw new InvalidDataException("Invalid source blob row");
                    var digest = Convert.ToHexStringLower(SHA256.HashData(rows.Bytes(1)));
                    sourceHashes.Add(rows.Text(0), (digest, rows.Bytes(1).Length));
                }
            }
            hash.AppendData("\n"u8);
        }
        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }

    internal static void AppendFrame(IncrementalHash hash, ReadOnlySpan<byte> bytes)
    {
        Span<byte> length = stackalloc byte[32];
        Utf8Formatter.TryFormat(bytes.Length, length, out var count);
        length[count++] = (byte)':';
        hash.AppendData(length[..count]); hash.AppendData(bytes);
    }

    public void Dispose() { if (handle != 0) { Native.sqlite3_close_v2(handle); handle = 0; } }

    internal static class Native
    {
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern int sqlite3_open_v2(byte* filename, nint* database, int flags, byte* vfs);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern int sqlite3_close_v2(nint database);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern byte* sqlite3_errmsg(nint database);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern int sqlite3_prepare_v2(nint database, byte* sql, int bytes, nint* statement, byte** tail);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern int sqlite3_step(nint statement);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern int sqlite3_reset(nint statement);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern int sqlite3_clear_bindings(nint statement);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern int sqlite3_finalize(nint statement);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern int sqlite3_bind_int64(nint statement, int index, long value);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern int sqlite3_bind_text(nint statement, int index, byte* value, int bytes, nint destructor);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern int sqlite3_column_count(nint statement);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern int sqlite3_column_type(nint statement, int column);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern long sqlite3_column_int64(nint statement, int column);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern double sqlite3_column_double(nint statement, int column);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern byte* sqlite3_column_text(nint statement, int column);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern byte* sqlite3_column_blob(nint statement, int column);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern int sqlite3_column_bytes(nint statement, int column);
    }
}

internal sealed unsafe class SqliteStatement : IDisposable
{
    private readonly SqliteDatabase database;
    private nint handle;
    internal SqliteStatement(SqliteDatabase database, nint handle) { this.database = database; this.handle = handle; }
    public int ColumnCount => SqliteDatabase.Native.sqlite3_column_count(handle);
    public bool Step()
    {
        ObjectDisposedException.ThrowIf(handle == 0, this);
        var result = SqliteDatabase.Native.sqlite3_step(handle);
        return result switch { 100 => true, 101 => false, _ => throw database.Error(result) };
    }
    public void Reset()
    {
        var result = SqliteDatabase.Native.sqlite3_reset(handle);
        if (result != 0) throw database.Error(result);
        result = SqliteDatabase.Native.sqlite3_clear_bindings(handle);
        if (result != 0) throw database.Error(result);
    }
    public void Bind(int index, long value) { var result = SqliteDatabase.Native.sqlite3_bind_int64(handle, index, value); if (result != 0) throw database.Error(result); }
    public void Bind(int index, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        fixed (byte* text = bytes)
        {
            var result = SqliteDatabase.Native.sqlite3_bind_text(handle, index, text, bytes.Length, -1 /* SQLITE_TRANSIENT */);
            if (result != 0) throw database.Error(result);
        }
    }
    public int Type(int column) => SqliteDatabase.Native.sqlite3_column_type(handle, column);
    public long Integer(int column) => SqliteDatabase.Native.sqlite3_column_int64(handle, column);
    public double Real(int column) => SqliteDatabase.Native.sqlite3_column_double(handle, column);
    public ReadOnlySpan<byte> Bytes(int column)
    {
        var pointer = Type(column) == 3 ? SqliteDatabase.Native.sqlite3_column_text(handle, column) : SqliteDatabase.Native.sqlite3_column_blob(handle, column);
        return new ReadOnlySpan<byte>(pointer, SqliteDatabase.Native.sqlite3_column_bytes(handle, column));
    }
    public string Text(int column)
    {
        if (Type(column) == 5) throw new InvalidDataException("Unexpected NULL SQLite text value");
        return new UTF8Encoding(false, true).GetString(Bytes(column));
    }
    public void HashCell(int column, IncrementalHash hash)
    {
        Span<byte> number = stackalloc byte[64];
        switch (Type(column))
        {
            case 5: hash.AppendData("N"u8); break;
            case 1:
                hash.AppendData("I"u8); Utf8Formatter.TryFormat(Integer(column), number, out var length); number[length++] = (byte)';'; hash.AppendData(number[..length]); break;
            case 2:
                hash.AppendData("F"u8); BinaryPrimitives.WriteDoubleLittleEndian(number, Real(column)); hash.AppendData(number[..8]); break;
            case 3: hash.AppendData("T"u8); SqliteDatabase.AppendFrame(hash, Bytes(column)); break;
            case 4: hash.AppendData("B"u8); SqliteDatabase.AppendFrame(hash, Bytes(column)); break;
            default: throw new InvalidDataException("Unknown SQLite storage class");
        }
    }
    public void Dispose() { if (handle != 0) { SqliteDatabase.Native.sqlite3_finalize(handle); handle = 0; } }
}

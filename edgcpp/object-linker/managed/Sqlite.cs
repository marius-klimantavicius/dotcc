using System.Reflection;
using System.Runtime.InteropServices;

namespace DotObj;

// SQLite is a linker dependency only. No generated application references this class.
internal sealed partial class Sqlite : IDisposable
{
    private nint database;

    static Sqlite()
    {
        NativeLibrary.SetDllImportResolver(typeof(Sqlite).Assembly, Resolve);
    }

    private static nint Resolve(string name, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (name != "dotobj_sqlite") return 0;
        string[] names = OperatingSystem.IsWindows() ? ["sqlite3.dll"]
            : OperatingSystem.IsMacOS() ? ["libsqlite3.dylib", "/usr/lib/libsqlite3.dylib"]
            : ["libsqlite3.so.0", "libsqlite3.so"];
        foreach (string candidate in names)
            if (NativeLibrary.TryLoad(candidate, assembly, searchPath, out nint handle)) return handle;
        throw new DllNotFoundException("The dotobj linker requires SQLite 3 (sqlite3.dll, libsqlite3.so, or libsqlite3.dylib).");
    }

    public Sqlite(string path) : this(path, false) {}

    private Sqlite(string path, bool catalogWriter)
    {
        // Object files stay READONLY; only the exclusive-create catalog factory
        // selects writing. No URI interpretation or extension loading in either mode.
        int result = Native.sqlite3_open_v2(path, out database, catalogWriter ? 2 : 1, 0);
        if (result != 0)
        {
            string error = Error;
            Dispose();
            throw new InvalidDataException($"Cannot open object {path}: {error}");
        }
        if (!catalogWriter) Execute("PRAGMA query_only=ON");
        else Execute("PRAGMA foreign_keys=ON");
        Execute("PRAGMA cache_size=-8192");
        Execute("PRAGMA temp_store=FILE");
        Execute("PRAGMA mmap_size=0");
        Execute("PRAGMA trusted_schema=OFF");
        if (!catalogWriter) Execute("BEGIN"); // All object reads observe a single snapshot.
    }

    private string Error => Marshal.PtrToStringUTF8(Native.sqlite3_errmsg(database)) ?? "SQLite error";

    public void Execute(string sql)
    {
        using var statement = Prepare(sql);
        while (statement.Read()) { }
    }

    public Statement Prepare(string sql)
    {
        if (Native.sqlite3_prepare_v2(database, sql, -1, out nint statement, 0) != 0)
            throw new InvalidDataException(Error);
        return new Statement(this, statement);
    }

    public void Dispose()
    {
        if (database != 0)
        {
            Native.sqlite3_close_v2(database);
            database = 0;
        }
    }

    internal sealed partial class Statement(Sqlite owner, nint handle) : IDisposable
    {
        private nint statement = handle;
        public bool Read()
        {
            int result = Native.sqlite3_step(statement);
            if (result == 100) return true;
            if (result == 101) return false;
            throw new InvalidDataException(owner.Error);
        }
        public long Integer(int column) => Native.sqlite3_column_int64(statement, column);
        public int ColumnCount => Native.sqlite3_column_count(statement);
        public int ColumnType(int column) => Native.sqlite3_column_type(statement, column);
        public bool IsNull(int column) => Native.sqlite3_column_type(statement, column) == 5;
        public string Text(int column)
        {
            if (IsNull(column)) throw new InvalidDataException($"Unexpected SQL NULL at column {column}");
            return Marshal.PtrToStringUTF8(Native.sqlite3_column_text(statement, column), Native.sqlite3_column_bytes(statement, column)) ?? "";
        }
        public string? OptionalText(int column) => IsNull(column) ? null : Text(column);
        public long? OptionalInteger(int column) => IsNull(column) ? null : Integer(column);
        public byte[] Bytes(int column)
        {
            int length = Native.sqlite3_column_bytes(statement, column);
            byte[] bytes = new byte[length];
            if (length != 0) Marshal.Copy(Native.sqlite3_column_blob(statement, column), bytes, 0, length);
            return bytes;
        }
        public void Dispose()
        {
            if (statement != 0) { Native.sqlite3_finalize(statement); statement = 0; }
        }
    }

    private static partial class Native
    {
        private const string Library = "dotobj_sqlite";
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_open_v2([MarshalAs(UnmanagedType.LPUTF8Str)] string path, out nint database, int flags, nint vfs);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_close_v2(nint database);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern nint sqlite3_errmsg(nint database);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_prepare_v2(nint database, [MarshalAs(UnmanagedType.LPUTF8Str)] string sql, int bytes, out nint statement, nint tail);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_step(nint statement);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_finalize(nint statement);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long sqlite3_column_int64(nint statement, int column);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_column_type(nint statement, int column);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern nint sqlite3_column_text(nint statement, int column);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_column_bytes(nint statement, int column);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_column_count(nint statement);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern nint sqlite3_column_blob(nint statement, int column);
    }
}

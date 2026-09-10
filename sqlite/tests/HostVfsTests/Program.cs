using System.Runtime.InteropServices;
using System.Text;
using DotCC.Sqlite;
using static DotCcLib;

internal static unsafe class Program
{
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static byte[] Utf8(string text) => Encoding.UTF8.GetBytes(text + "\0");
    private static sqlite3* OpenDatabase(string path, int flags = 6)
    {
        sqlite3* db = null;
        fixed (byte* name = Utf8(path))
        {
            var result = sqlite3_open_v2(name, &db, flags, null);
            if (result != 0)
            {
                var message = Marshal.PtrToStringUTF8((nint)sqlite3_errmsg(db));
                if (db != null) sqlite3_close(db);
                throw new InvalidOperationException($"Open {path}: {result} {message}");
            }
        }
        return db;
    }

    private static int Execute(sqlite3* db, string sql)
    {
        fixed (byte* text = Utf8(sql)) return sqlite3_exec(db, text, null, null, null);
    }

    private static string Query(sqlite3* db, string sql)
    {
        sqlite3_stmt* statement = null;
        fixed (byte* text = Utf8(sql))
            Require(sqlite3_prepare_v2(db, text, -1, &statement, null) == 0, "prepare: " + sql);
        try
        {
            Require(sqlite3_step(statement) == 100, "query row: " + sql);
            var result = Marshal.PtrToStringUTF8((nint)sqlite3_column_text(statement, 0)) ?? "NULL";
            Require(sqlite3_step(statement) == 101, "query done: " + sql);
            return result;
        }
        finally { Require(sqlite3_finalize(statement) == 0, "finalize"); }
    }

    private static void Serve(string path, bool readOnly)
    {
        var db = OpenDatabase(path, readOnly ? 1 : 6);
        try
        {
            Console.WriteLine("READY");
            Console.Out.Flush();
            string? line;
            while ((line = Console.ReadLine()) != null)
            {
                if (line == "X") break;
                if (line.StartsWith("E ", StringComparison.Ordinal)) Console.WriteLine("RC " + Execute(db, line[2..]));
                else if (line.StartsWith("Q ", StringComparison.Ordinal)) Console.WriteLine("ROW " + Query(db, line[2..]));
                else throw new InvalidOperationException("Unknown protocol command");
                Console.Out.Flush();
            }
        }
        finally { Require(sqlite3_close(db) == 0, "server close"); }
    }

    private static sqlite3_file* OpenRaw(sqlite3_vfs* vfs, string? path, int flags, out int actualFlags)
    {
        var file = (sqlite3_file*)NativeMemory.AllocZeroed((nuint)vfs->szOsFile);
        var result = 0;
        int returned = 0;
        fixed (byte* name = path == null ? null : Utf8(path)) result = vfs->xOpen(vfs, name, file, flags, &returned);
        actualFlags = returned;
        if (result != 0) { NativeMemory.Free(file); throw new InvalidOperationException($"raw open: {result}"); }
        return file;
    }

    private static void CloseRaw(sqlite3_file* file)
    {
        var result = file->pMethods->xClose(file);
        Require(file->pMethods == null, "closed handle must clear methods");
        NativeMemory.Free(file);
        Require(result == 0, "raw close");
    }

    private static void RawContracts(string directory)
    {
        var vfs = sqlite3_vfs_find(null);
        Require(Marshal.PtrToStringUTF8((nint)vfs->zName) == "dotcc-host", "host default registration");
        Require(vfs->iVersion == 2 && vfs->xCurrentTimeInt64 != null, "real clock API");
        var path = Path.Combine(directory, "raw-λ.db");
        var first = OpenRaw(vfs, path, 6 | 0x100, out var flags);
        Require((flags & 3) == 2, "readwrite returned flag");
        var second = OpenRaw(vfs, path, 2 | 0x100, out _);
        var readOnly = OpenRaw(vfs, path, 1 | 0x100, out flags);
        try
        {
            Require((flags & 3) == 1, "readonly returned flag");
            Require(first->pMethods->iVersion == 2 && first->pMethods->xShmMap != null && first->pMethods->xShmLock != null && first->pMethods->xShmBarrier != null && first->pMethods->xShmUnmap != null, "WAL I/O methods");
            byte* data = stackalloc byte[8];
            for (int i = 0; i < 8; i++) data[i] = (byte)(i + 1);
            Require(first->pMethods->xWrite(first, data, 8, 4096) == 0, "offset write");
            long size = -1;
            Require(first->pMethods->xFileSize(first, &size) == 0 && size == 4104, "size after sparse write");
            new Span<byte>(data, 8).Fill(255);
            Require(second->pMethods->xRead(second, data, 8, 4096) == 0 && data[0] == 1 && data[7] == 8, "other handle sees writes");
            Require(second->pMethods->xRead(second, data, 8, 4100) == (10 | 2 << 8), "short read code");
            Require(data[0] == 5 && data[3] == 8 && data[4] == 0 && data[7] == 0, "short read zero fill");
            Require(second->pMethods->xRead(second, data, 8, 0) == 0 && new ReadOnlySpan<byte>(data, 8).IndexOfAnyExcept((byte)0) == -1, "sparse hole zeros");
            Require(readOnly->pMethods->xWrite(readOnly, data, 1, 0) == 8, "readonly write rejected");
            Require(readOnly->pMethods->xTruncate(readOnly, 0) == 8, "readonly truncate rejected");
            Require(first->pMethods->xTruncate(first, 4) == 0 && first->pMethods->xFileSize(first, &size) == 0 && size == 4, "truncate length");
            Require(first->pMethods->xSync(first, 3) == 0, "full disk sync");
            Require(first->pMethods->xRead(first, data, 8, long.MaxValue) != 0, "invalid read range");
            Require(first->pMethods->xWrite(first, data, 8, -1) != 0, "invalid write range");
            Require(first->pMethods->xLock(first, 1) == 0 && second->pMethods->xLock(second, 1) == 0, "shared coexistence");
            Require(readOnly->pMethods->xLock(readOnly, 1) == 0, "readonly shared");
            Require(first->pMethods->xLock(first, 2) == 0, "reserved upgrade");
            int reserved = 0;
            Require(readOnly->pMethods->xCheckReservedLock(readOnly, &reserved) == 0 && reserved == 1, "readonly reserved probe");
            Require(second->pMethods->xLock(second, 2) == 5, "two reserved writers conflict");
            Require(first->pMethods->xLock(first, 4) == 5, "exclusive blocked by readers");
            int level = 0;
            Require(first->pMethods->xFileControl(first, 1, &level) == 0 && level == 3, "failed upgrade retains pending");
            var late = OpenRaw(vfs, path, 2 | 0x100, out _);
            try { Require(late->pMethods->xLock(late, 1) == 5, "pending excludes new reader"); }
            finally { CloseRaw(late); }
            Require(second->pMethods->xUnlock(second, 0) == 0 && readOnly->pMethods->xUnlock(readOnly, 0) == 0, "release readers");
            Require(first->pMethods->xLock(first, 4) == 0, "exclusive after readers leave");
            Require(first->pMethods->xUnlock(first, 1) == 0, "exclusive downgrade");
            Require(second->pMethods->xLock(second, 1) == 0, "reader after downgrade");
            Require(second->pMethods->xCheckReservedLock(second, &reserved) == 0 && reserved == 0, "downgrade releases reserved");
            Require(first->pMethods->xUnlock(first, 0) == 0 && second->pMethods->xUnlock(second, 0) == 0, "release all locks");
            Require(first->pMethods->xFileControl(first, 987654, &level) == 12, "unknown file control");
        }
        finally { CloseRaw(first); CloseRaw(second); CloseRaw(readOnly); }

        var temporary = OpenRaw(vfs, null, 6 | 8 | 0x200, out _);
        CloseRaw(temporary);
        var deletePath = Path.Combine(directory, "delete-on-close.tmp");
        var deleted = OpenRaw(vfs, deletePath, 6 | 8 | 16 | 0x200, out _);
        CloseRaw(deleted);
        Require(!File.Exists(deletePath), "delete-on-close removes named temp");
        var failed = (sqlite3_file*)NativeMemory.AllocZeroed((nuint)vfs->szOsFile);
        try
        {
            fixed (byte* name = Utf8(path))
                Require(vfs->xOpen(vfs, name, failed, 6 | 16 | 0x100, null) != 0 && failed->pMethods == null, "exclusive creation cannot overwrite");
            fixed (byte* name = Utf8(Path.Combine(directory, "missing", "db")))
                Require(vfs->xOpen(vfs, name, failed, 6 | 0x100, null) != 0 && failed->pMethods == null, "failed open clears methods");
        }
        finally { NativeMemory.Free(failed); }
        if (!OperatingSystem.IsWindows())
        {
            var link = Path.Combine(directory, "nofollow-link");
            File.CreateSymbolicLink(link, path);
            sqlite3* refused = null;
            fixed (byte* name = Utf8(link))
            {
                var rc = sqlite3_open_v2(name, &refused, 6 | 0x1000000, null);
                Require((rc & 255) == 14, "OPEN_NOFOLLOW rejects canonicalized symlink");
            }
            if (refused != null) Require(sqlite3_close(refused) == 0, "close failed nofollow database");
            var rawRefused = (sqlite3_file*)NativeMemory.AllocZeroed((nuint)vfs->szOsFile);
            try
            {
                fixed (byte* name = Utf8(link))
                    Require(vfs->xOpen(vfs, name, rawRefused, 6 | 0x100 | 0x1000000, null) != 0
                        && rawRefused->pMethods == null, "atomic OS nofollow for raw open");
            }
            finally { NativeMemory.Free(rawRefused); }
        }
        fixed (byte* name = Utf8(path))
        {
            byte* fullPath = stackalloc byte[32768];
            Require(vfs->xFullPathname(vfs, name, 2, fullPath) != 0, "short path buffer rejected");
            var pathResult = vfs->xFullPathname(vfs, name, 32768, fullPath);
            var canonical = Marshal.PtrToStringUTF8((nint)fullPath)!;
            Require(pathResult is 0 or 512 && Path.IsPathFullyQualified(canonical) && File.Exists(canonical)
                && Path.GetFileName(canonical) == Path.GetFileName(path), "UTF8 full path");
            int exists = 0;
            Require(vfs->xAccess(vfs, name, 0, &exists) == 0 && exists == 1, "access existing");
            Require(vfs->xDelete(vfs, name, 1) == 0 && !File.Exists(path), "delete plus directory sync");
        }
        long now = 0;
        Require(vfs->xCurrentTimeInt64(vfs, &now) == 0 && Math.Abs(now - 210866760000000L - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) < 5000, "wall clock");
        byte* random = stackalloc byte[64];
        Require(vfs->xRandomness(vfs, 64, random) == 64 && new ReadOnlySpan<byte>(random, 64).IndexOfAnyExcept((byte)0) >= 0, "OS randomness");
        Require(HostVfs.OpenHandleCount == 0, "raw handles released");
        Console.WriteLine("PASS host VFS raw I/O, locking, readonly, paths, temporary files, flushes, clocks and cleanup");
    }

    private static void SqlContracts(string directory)
    {
        var path = Path.Combine(directory, "sql-Ω.db");
        var db = OpenDatabase(path);
        try
        {
            Require(Query(db, "PRAGMA journal_mode") == "delete", "rollback journal default");
            Require(Execute(db, "PRAGMA synchronous=FULL;CREATE TABLE data(id INTEGER PRIMARY KEY, value BLOB);INSERT INTO data VALUES(1,jsonb('{\"name\":\"λ\",\"n\":42}'));CREATE VIRTUAL TABLE docs USING fts5(body);INSERT INTO docs VALUES('café database search');") == 0, "SQL/JSONB/FTS create");
            Require(Execute(db, "BEGIN;UPDATE data SET value=jsonb('{}');ROLLBACK;") == 0, "rollback");
        }
        finally { Require(sqlite3_close(db) == 0, "SQL close"); }
        Require(File.ReadAllBytes(path).AsSpan(0, 16).SequenceEqual("SQLite format 3\0"u8), "real SQLite file header");
        db = OpenDatabase(path, 1);
        try
        {
            Require(Query(db, "SELECT json_extract(value,'$.name') FROM data") == "λ", "JSONB reopen");
            Require(Query(db, "SELECT count(*) FROM docs WHERE docs MATCH 'cafe'") == "1", "FTS index reopen");
            Require(Execute(db, "INSERT INTO data VALUES(2,NULL)") == 8, "readonly SQL cannot write");
            Require(Query(db, "PRAGMA integrity_check") == "ok", "disk integrity");
        }
        finally { Require(sqlite3_close(db) == 0, "readonly close"); }
        Require(HostVfs.OpenHandleCount == 0, "SQL handles released");
        Require(sqlite3_shutdown() == 0 && sqlite3_initialize() == 0, "VFS shutdown/reinitialize");
        Require(Marshal.PtrToStringUTF8((nint)sqlite3_vfs_find(null)->zName) == "dotcc-host", "default survives reinitialize");
        Console.WriteLine("PASS host VFS SQL/JSONB/FTS5 disk persistence, rollback, readonly and reinitialization");
    }

    private static int Main(string[] args)
    {
        Console.InputEncoding = new UTF8Encoding(false);
        Console.OutputEncoding = new UTF8Encoding(false);
        try
        {
            Require(sqlite3_initialize() == 0, "SQLite initialization");
            if (args.Length >= 2 && args[0] == "serve") Serve(args[1], args.Length > 2 && args[2] == "readonly");
            else
            {
                var directory = Path.Combine(Path.GetTempPath(), "dotcc-host-tests-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(directory);
                try { RawContracts(directory); SqlContracts(directory); }
                finally { Directory.Delete(directory, recursive: true); }
            }
            Require(HostVfs.OpenHandleCount == 0 && sqlite3_shutdown() == 0, "final shutdown");
            return 0;
        }
        catch (Exception exception) { Console.Error.WriteLine(exception); return 1; }
    }
}

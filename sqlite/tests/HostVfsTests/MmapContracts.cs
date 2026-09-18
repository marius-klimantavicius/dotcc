using System.Runtime.InteropServices;
using Managed.Database;
using static global::Managed.Database.Sqlite;

internal static unsafe partial class Program
{
    private static void RawMappingContracts(string directory)
    {
        var vfs = sqlite3_vfs_find(null);
        var path = Path.Combine(directory, "raw-mmap.db");
        var first = OpenRaw(vfs, path, 6 | 0x100, out _);
        var second = OpenRaw(vfs, path, 1 | 0x100, out _);
        try
        {
            var methods = first->pMethods;
            Require(methods->iVersion == 3 && methods->xFetch != null && methods->xUnfetch != null, "mmap I/O methods");
            var data = new byte[32768];
            for (int i = 0; i < data.Length; ++i) data[i] = (byte)(i % 251);
            fixed (byte* bytes = data) Require(methods->xWrite(first, bytes, data.Length, 0) == 0, "mmap seed file");
            long limit = -1;
            Require(methods->xFileControl(first, 18, &limit) == 0 && limit > 0, "default mmap enabled");
            var initial = limit;
            limit = 8192;
            Require(methods->xFileControl(first, 18, &limit) == 0 && limit == initial, "mmap cap returns old value");
            void* firstPage = null;
            void* duplicate = null;
            void* lastPage = null;
            Require(methods->xFetch(first, 0, 4096, &firstPage) == 0 && firstPage != null, "mapped first page");
            Require(new ReadOnlySpan<byte>(firstPage, 4096).SequenceEqual(data.AsSpan(0, 4096)), "mapped file bytes");
            Require(methods->xFetch(first, 0, 4096, &duplicate) == 0 && duplicate == firstPage, "repeated fetch stable address");
            Require(methods->xFetch(first, 4096, 4096, &lastPage) == 0 && lastPage == null, "256 byte overread guard at cap");
            limit = 0;
            Require(methods->xFileControl(first, 18, &limit) == 0 && limit == 8192, "live borrow prevents disabling mmap");
            limit = -1;
            Require(methods->xFileControl(first, 18, &limit) == 0 && limit == 8192, "cap unchanged while pinned");
            Require(methods->xUnfetch(first, 1, firstPage) != 0, "wrong offset cannot release borrow");
            Require(methods->xUnfetch(first, 0, null) == 5, "cannot invalidate borrowed view");
            Require(methods->xTruncate(first, 1024) == 5, "cannot truncate borrowed view");
            GC.Collect();
            Require(((byte*)firstPage)[250] == 250, "borrow remains valid across GC and rejected operations");
            Require(methods->xUnfetch(first, 0, firstPage) == 0 && methods->xUnfetch(first, 0, duplicate) == 0, "paired duplicate release");
            Require(HostVfs.MappedReferenceCount == 0, "all raw duplicate borrows released");
            limit = 1048576;
            Require(methods->xFileControl(first, 18, &limit) == 0, "raise mmap cap");
            Require(methods->xFetch(first, 4096, 4096, &firstPage) == 0 && firstPage != null, "map beyond old cap");
            byte value = 177;
            Require(methods->xWrite(first, &value, 1, 4096) == 0, "write through ordinary I/O");
            Thread.MemoryBarrier();
            Require(*(byte*)firstPage == value, "mapped read observes file write");
            Require(methods->xWrite(first, &value, 1, 65535) == 0, "grow while old view borrowed");
            Require(methods->xFetch(first, 49152, 4096, &lastPage) == 0 && lastPage == null, "growth falls back while borrowed");
            Require(*(byte*)firstPage == value, "growth preserves old address");
            Require(methods->xUnfetch(first, 4096, firstPage) == 0, "release before remap");
            Require(methods->xFetch(first, 49152, 4096, &lastPage) == 0 && lastPage != null, "remap grown file");
            Require(*(byte*)lastPage == 0, "file growth hole reads zero");
            Require(methods->xUnfetch(first, 49152, lastPage) == 0, "release grown mapping");
            Require(methods->xTruncate(first, 8192) == 0, "truncate releases idle view");
            Require(methods->xFetch(first, 4096, 4096, &lastPage) == 0 && lastPage == null, "EOF overread guard");
            Require(methods->xFetch(first, long.MaxValue - 8, 16, &lastPage) != 0 && lastPage == null, "overflow fetch rejected");
            Require(second->pMethods->xFetch(second, 0, 4096, &firstPage) == 0 && firstPage != null, "readonly file mapping");
            Require(second->pMethods->xUnfetch(second, 0, firstPage) == 0, "readonly mapping release");
            limit = 0;
            Require(methods->xFileControl(first, 18, &limit) == 0, "disable mmap");
            Require(methods->xFetch(first, 0, 4096, &firstPage) == 0 && firstPage == null, "disabled mapping falls back");
            Require(methods->xUnfetch(first, 0, null) == 0, "idle invalidation");
            limit = long.MaxValue;
            Require(methods->xFileControl(first, 18, &limit) == 0 && limit == 0, "oversized request returns old cap");
            limit = -1;
            Require(methods->xFileControl(first, 18, &limit) == 0 && limit == Sqlite.Globals.sqlite3Config.mxMmap, "mmap cap clamps to configured maximum");
        }
        finally { CloseRaw(first); CloseRaw(second); }
        Require(HostVfs.MappedViewCount == 0 && HostVfs.MappedReferenceCount == 0, "raw mappings reclaimed");
        Console.WriteLine("PASS mmap raw cap/query, borrowed pointers, duplicate release, EOF guard, growth, truncation, readonly and cleanup");
    }

    private static void SqlMappingContracts(string directory)
    {
        foreach (var mode in new[] { "DELETE", "WAL" })
        {
            var path = Path.Combine(directory, "sql-mmap-" + mode + ".db");
            var writer = OpenDatabase(path);
            sqlite3* reader = null;
            try
            {
                Require(Query(writer, "PRAGMA journal_mode=" + mode) == mode.ToLowerInvariant(), "mmap journal mode");
                Require(Execute(writer, "CREATE TABLE mapped(id INTEGER PRIMARY KEY,body TEXT); WITH RECURSIVE t(x) AS (VALUES(1) UNION ALL SELECT x+1 FROM t WHERE x<512) INSERT INTO mapped SELECT x,printf('%01000d',x) FROM t;") == 0, "mmap seed SQL");
                if (mode == "WAL") Require(Execute(writer, "PRAGMA wal_checkpoint(TRUNCATE)") == 0, "materialize WAL database pages");
                reader = OpenDatabase(path);
                Require(Query(reader, "PRAGMA mmap_size=1048576") == "1048576", "requested mmap size");
                Require(Execute(reader, "PRAGMA cache_size=8; BEGIN;") == 0, "mmap read transaction");
                var before = HostVfs.MappedReadCount;
                Require(Query(reader, "SELECT sum(length(body)) FROM mapped") == "512000", "mapped SQL bytes");
                Require(HostVfs.MappedReadCount > before, "SQL actually used xFetch");
                if (mode == "WAL")
                {
                    Require(Execute(writer, "INSERT INTO mapped VALUES(513,printf('%01000d',513))") == 0, "concurrent WAL write");
                    Require(Query(reader, "SELECT count(*) FROM mapped") == "512", "mapped WAL snapshot");
                }
                else Require(Execute(writer, "INSERT INTO mapped VALUES(513,printf('%01000d',513))") == 5, "rollback reader blocks writer");
                Require(Execute(reader, "COMMIT") == 0, "mmap end snapshot");
                if (mode == "DELETE") Require(Execute(writer, "INSERT INTO mapped VALUES(513,printf('%01000d',513))") == 0, "rollback write after reader");
                if (mode == "WAL") Require(Execute(writer, "PRAGMA wal_checkpoint(TRUNCATE)") == 0, "mmap checkpoint");
                Require(Query(reader, "SELECT sum(length(body)) FROM mapped") == "513000", "remapped updated data");
                Require(Query(reader, "PRAGMA mmap_size=0") == "0", "disable SQL mmap");
                before = HostVfs.MappedReadCount;
                Require(Query(reader, "SELECT sum(length(body)) FROM mapped") == "513000", "fallback SQL bytes");
                Require(HostVfs.MappedReadCount == before, "disabled SQL does not fetch mappings");
                Require(Execute(writer, "DELETE FROM mapped WHERE id>4; VACUUM;") == 0, "shrink with sibling connection open");
                Require(Query(reader, "PRAGMA mmap_size=1048576") == "1048576", "reenable SQL mmap");
                Require(Query(reader, "SELECT sum(length(body)) FROM mapped") == "4000", "mapping after shrink");
                Require(Query(reader, "PRAGMA integrity_check") == "ok", "mapped integrity");
                Require(sqlite3_close(reader) == 0, "mmap reader close"); reader = null;
                Require(sqlite3_close(writer) == 0, "mmap writer close"); writer = null;
                reader = OpenDatabase(path, 1);
                Require(Query(reader, "PRAGMA mmap_size=1048576") == "1048576", "readonly mmap pragma");
                Require(Query(reader, "SELECT sum(length(body)) FROM mapped") == "4000", "readonly mapped reopen");
            }
            finally { if (reader != null) sqlite3_close(reader); if (writer != null) sqlite3_close(writer); }
            Require(HostVfs.MappedViewCount == 0 && HostVfs.MappedReferenceCount == 0, "SQL mappings reclaimed");
        }
        Console.WriteLine("PASS mmap SQL actual fetches, rollback/WAL snapshots, growth, checkpoint, fallback, VACUUM and readonly reopen");
    }
}

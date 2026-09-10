using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using Managed.Database.UpstreamUnix;
using static Managed.Database.UpstreamUnix.Sqlite;

unsafe class Program
{
    static void Require(bool value, string message) { if (!value) throw new Exception(message); }
    static string Text(byte* p) => Marshal.PtrToStringUTF8((nint)p) ?? "";
    static void Check(int rc, sqlite3* db) { if (rc != 0) throw new Exception($"SQLite {rc}: {Text(sqlite3_errmsg(db))}"); }
    static sqlite3* Open(string path, string vfs = "unix") {
        sqlite3* db = null;
        fixed (byte* p = Encoding.UTF8.GetBytes(path + "\0"))
        fixed (byte* v = Encoding.UTF8.GetBytes(vfs + "\0"))
            Check(sqlite3_open_v2(p, &db, 6, v), db);
        return db;
    }
    static int Exec(sqlite3* db, string sql) {
        fixed (byte* p = Encoding.UTF8.GetBytes(sql + "\0")) return sqlite3_exec(db, p, null, null, null);
    }
    static string Scalar(sqlite3* db, string sql) {
        sqlite3_stmt* stmt = null;
        fixed (byte* p = Encoding.UTF8.GetBytes(sql + "\0")) Check(sqlite3_prepare_v2(db, p, -1, &stmt, null), db);
        try { Require(sqlite3_step(stmt) == 100, sql); return Text(sqlite3_column_text(stmt, 0)); }
        finally { Check(sqlite3_finalize(stmt), db); }
    }
    static void Main(string[] args) {
        Require(OperatingSystem.IsLinux() && RuntimeInformation.ProcessArchitecture == Architecture.X64, "Experimental Linux x64 profile required");
        if (args.Length > 0 && args[0] == "--contend") {
            var child = Open(args[1]);
            try { Require(Exec(child, "BEGIN IMMEDIATE") == 5, "cross-process writer contention"); }
            finally { Check(sqlite3_close(child), child); sqlite3_shutdown(); }
            return;
        }
        uvfs_stat layout = default;
        Require(sizeof(uvfs_stat) == 96 && (byte*)&layout.st_size - (byte*)&layout == 40, "OS stat bridge layout");
        var path = Path.Combine(Path.GetTempPath(), "dotcc-upstream-vfs-" + Guid.NewGuid().ToString("N") + ".db");
        sqlite3* a = null; sqlite3* b = null;
        try {
            a = Open(path); b = Open(path);
            Require(Text(sqlite3_vfs_find(null)->zName) == "unix", "upstream Unix VFS default");
            Check(Exec(a, "CREATE TABLE t(id INTEGER PRIMARY KEY, v TEXT); INSERT INTO t VALUES(1,'first'),(2,'second');"), a);
            Require(Scalar(b, "SELECT count(*) FROM t") == "2", "disk visibility");
            Check(Exec(a, "BEGIN IMMEDIATE; UPDATE t SET v='changed' WHERE id=1;"), a);
            Require(Exec(b, "BEGIN IMMEDIATE") == 5, "rollback writer locking");
            var start = new ProcessStartInfo(Environment.ProcessPath!) { UseShellExecute=false };
            if (Path.GetFileNameWithoutExtension(Environment.ProcessPath) == "dotnet") start.ArgumentList.Add(Environment.GetCommandLineArgs()[0]);
            start.ArgumentList.Add("--contend"); start.ArgumentList.Add(path);
            using (var child=Process.Start(start)!) {
                if (!child.WaitForExit(30000)) { child.Kill(entireProcessTree:true); child.WaitForExit(); throw new Exception("child lock test timeout"); }
                Require(child.ExitCode==0,"child lock test");
            }
            Check(Exec(a, "ROLLBACK"), a);
            Require(Scalar(a, "PRAGMA journal_mode=WAL") == "wal", "WAL enabled");
            Check(Exec(b, "BEGIN; SELECT * FROM t;"), b);
            Check(Exec(a, "INSERT INTO t VALUES(3,'third')"), a);
            Require(Scalar(b, "SELECT count(*) FROM t") == "2", "WAL snapshot");
            Check(Exec(b, "COMMIT"), b);
            Require(Scalar(b, "SELECT count(*) FROM t") == "3", "WAL visibility");
            Require(Scalar(a, "PRAGMA mmap_size=67108864") == "67108864", "mmap cap");
            Check(Exec(a, "PRAGMA wal_checkpoint(TRUNCATE); VACUUM;"), a);
            sqlite3_file* file = null;
            Check(sqlite3_file_control(a, null, 7, &file), a);
            void* mapping = null;
            Check(file->pMethods->xFetch(file, 0, 16, &mapping), a);
            Require(mapping != null && Encoding.ASCII.GetString(new ReadOnlySpan<byte>(mapping,15)) == "SQLite format 3", "actual upstream mmap fetch");
            Check(file->pMethods->xUnfetch(file, 0, mapping), a);
            Require(Scalar(a, "SELECT json_extract(jsonb('{\"a\":42}'),'$.a')") == "42", "JSONB");
            Check(Exec(a, "CREATE VIRTUAL TABLE docs USING fts5(body); INSERT INTO docs VALUES('translated upstream vfs');"), a);
            Require(Scalar(a, "SELECT count(*) FROM docs WHERE docs MATCH 'upstream'") == "1", "FTS5");
            Check(Exec(a, "UPDATE t SET v=upper(v) WHERE id>=2; DELETE FROM t WHERE id=3;"), a);
            Require(Scalar(a, "PRAGMA integrity_check") == "ok", "integrity");
            Check(sqlite3_close(b), b); b=null; Check(sqlite3_close(a), a); a=null;
            a=Open(path); Require(Scalar(a, "SELECT v FROM t WHERE id=2") == "SECOND", "reopen persistence");
            Check(sqlite3_close(a), a); a=null;
            Require(sqlite3_shutdown() == 0, "shutdown");
            Require(HostMutex.DynamicMutexCount == 0, "mutex cleanup");
            Console.WriteLine("PASS translated upstream Unix VFS: disk CRUD, rollback/cross-process locking, WAL snapshots/checkpoint, actual mmap fetch, JSONB, FTS5, integrity, reopen and shutdown");
        } finally {
            if (b != null) sqlite3_close(b); if (a != null) sqlite3_close(a);
            foreach(var suffix in new[]{"", "-wal", "-shm", "-journal"}) File.Delete(path+suffix);
        }
    }
}

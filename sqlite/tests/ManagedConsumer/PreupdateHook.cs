using System.Runtime.InteropServices;
using static global::Managed.Database.Sqlite;

internal static unsafe partial class Program
{
    private static readonly delegate*<void*, sqlite3*, int, byte*, byte*, long, long, void>
        PreupdatePointer = &CapturePreupdate;

    private sealed record PreupdateEvent(int Operation, string Database, string Table,
        long? OldRowid, long? NewRowid, int Depth, string? OldValues, string? NewValues);

    private sealed class PreupdateState(nint database)
    {
        public readonly nint Database = database;
        public readonly List<PreupdateEvent> Events = [];
        public Exception? Failure;
    }

    private static void CheckPreupdateHook(sqlite3* db)
    {
        Expect(db, "SELECT sqlite_compileoption_used('ENABLE_PREUPDATE_HOOK')", "1");
        Execute(db, """
            CREATE TABLE preupdate_rows(id INTEGER PRIMARY KEY, label TEXT, amount INTEGER);
            CREATE TABLE preupdate_audit(id INTEGER PRIMARY KEY, label TEXT, amount INTEGER);
            CREATE TRIGGER preupdate_trigger AFTER UPDATE ON preupdate_rows BEGIN
              INSERT INTO preupdate_audit VALUES(new.id, new.label, new.amount);
            END;
            """);
        var state = new PreupdateState((nint)db);
        var handle = GCHandle.Alloc(state);
        void* context = (void*)GCHandle.ToIntPtr(handle);
        try
        {
            if (sqlite3_preupdate_hook(db, PreupdatePointer, context) != null)
                throw new InvalidOperationException("Unexpected existing preupdate hook");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Execute(db, """
                INSERT INTO preupdate_rows VALUES(1, 'before', NULL);
                UPDATE preupdate_rows SET id=2, label='après', amount=42 WHERE id=1;
                DELETE FROM preupdate_rows WHERE id=2;
                """);
            if (state.Failure != null)
                throw new InvalidOperationException("Preupdate callback failed", state.Failure);
            PreupdateEvent[] expected =
            [
                new(SQLITE_INSERT, "main", "preupdate_rows", null, 1, 0, null, "1|before|<NULL>"),
                new(SQLITE_UPDATE, "main", "preupdate_rows", 1, 2, 0, "1|before|<NULL>", "2|après|42"),
                new(SQLITE_INSERT, "main", "preupdate_audit", null, 2, 1, null, "2|après|42"),
                new(SQLITE_DELETE, "main", "preupdate_rows", 2, null, 0, "2|après|42", null),
            ];
            if (!state.Events.SequenceEqual(expected))
                throw new InvalidOperationException("Preupdate events mismatch: " + string.Join("; ", state.Events));
            if (sqlite3_preupdate_hook(db, null, null) != context)
                throw new InvalidOperationException("Preupdate unregister did not return the previous context");
            Execute(db, "INSERT INTO preupdate_rows VALUES(3, 'unregistered', 7)");
            if (state.Events.Count != expected.Length)
                throw new InvalidOperationException("Preupdate callback fired after unregistering");
            Expect(db, "SELECT id||'|'||label||'|'||amount FROM preupdate_audit", "2|après|42");
        }
        finally
        {
            sqlite3_preupdate_hook(db, null, null);
            handle.Free();
            Execute(db, "DROP TABLE preupdate_rows; DROP TABLE preupdate_audit;");
        }
        Console.WriteLine("managed preupdate hook: insert/update/delete, old/new values, rowids, trigger depth and unregister passed");
    }

    private static void CapturePreupdate(void* context, sqlite3* db, int operation,
        byte* database, byte* table, long oldRowid, long newRowid)
    {
        var state = (PreupdateState)GCHandle.FromIntPtr((nint)context).Target!;
        // Keep managed exceptions inside the callback; report them after sqlite3_exec returns.
        try
        {
            if ((nint)db != state.Database || sqlite3_preupdate_count(db) != 3
                || sqlite3_preupdate_blobwrite(db) != -1)
                throw new InvalidOperationException("Incorrect preupdate connection, column count or blob-write marker");
            state.Events.Add(new PreupdateEvent(operation, Utf8(database), Utf8(table),
                operation == SQLITE_INSERT ? null : oldRowid,
                operation == SQLITE_DELETE ? null : newRowid,
                sqlite3_preupdate_depth(db),
                operation == SQLITE_INSERT ? null : ReadPreupdateValues(db, old: true),
                operation == SQLITE_DELETE ? null : ReadPreupdateValues(db, old: false)));
        }
        catch (Exception error) { state.Failure ??= error; }
    }

    private static string ReadPreupdateValues(sqlite3* db, bool old)
    {
        var values = new string[sqlite3_preupdate_count(db)];
        for (int column = 0; column < values.Length; column++)
        {
            sqlite3_value* value = null;
            Check(old ? sqlite3_preupdate_old(db, column, &value)
                : sqlite3_preupdate_new(db, column, &value), db, "preupdate value");
            // Values belong to SQLite and expire as soon as the callback returns.
            values[column] = sqlite3_value_type(value) == SQLITE_NULL
                ? "<NULL>" : Utf8(sqlite3_value_text(value));
        }
        return string.Join("|", values);
    }
}

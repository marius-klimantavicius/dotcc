// Manual .NET adaptation of @electric-sql/pglite src/base.ts (Apache-2.0).
namespace Managed.PGlite;

/// <summary>A synchronous, owner-thread transaction scope. Every SQL method throws
/// on SQL errors; the token closes on commit, rollback or callback failure.</summary>
public sealed class PgliteTransaction
{
    private readonly PgliteDatabase database;
    public bool Closed { get; private set; }
    internal PgliteTransaction(PgliteDatabase database) { this.database = database; }
    internal void Complete() => Closed = true;
    internal void RequireOpen()
    { if (Closed) throw new InvalidOperationException("Transaction is closed."); }

    public PgliteQueryResult Query(string sql)
        => database.WithTransaction(this, () => { ValidateSql(sql); return database.Query(sql).EnsureSuccess(); });
    public PgliteQueryResult Query(string sql, IReadOnlyList<object?> parameters, PgliteQueryOptions? options = null)
        => database.WithTransaction(this, () => { ValidateSql(sql); return database.Query(sql, parameters, options).EnsureSuccess(); });
    public PgliteQueryResult Sql(PgliteSql sql)
        => database.WithTransaction(this, () => { ArgumentNullException.ThrowIfNull(sql); ValidateSql(sql.Text); return database.Sql(sql).EnsureSuccess(); });
    public PgliteQueryResult Exec(string sql)
        => database.WithTransaction(this, () => { ValidateSql(sql); return database.Exec(sql); });
    public PgliteSubscription Listen(string channel, Action<string> callback)
        => database.WithTransaction(this, () => database.Listen(channel, callback));
    public void Unlisten(string channel, Action<string>? callback = null)
        => database.WithTransaction(this, () => { database.Unlisten(channel, callback); return 0; });
    // Only scan statement starts, leaving SQL parsing and execution to PostgreSQL.
    // Prevent explicit outer transaction control from escaping this scope, including
    // commands after comments, quoted text, dollar-quoted function bodies or ';'.
    private void ValidateSql(string sql)
    {
        ArgumentNullException.ThrowIfNull(sql);
        bool standardStrings = database.UsesStandardStrings();
        bool start = true;
        for (int i = 0; i < sql.Length;)
        {
            char c = sql[i];
            if (char.IsWhiteSpace(c)) { i++; continue; }
            if (c == '-' && i + 1 < sql.Length && sql[i + 1] == '-')
            { i += 2; while (i < sql.Length && sql[i] is not ('\r' or '\n')) i++; continue; }
            if (c == '/' && i + 1 < sql.Length && sql[i + 1] == '*')
            {
                int depth = 1; i += 2;
                while (i < sql.Length && depth != 0)
                {
                    if (i + 1 < sql.Length && sql[i] == '/' && sql[i + 1] == '*') { depth++; i += 2; }
                    else if (i + 1 < sql.Length && sql[i] == '*' && sql[i + 1] == '/') { depth--; i += 2; }
                    else i++;
                }
                continue;
            }
            if (c == ';') { start = true; i++; continue; }
            if (c is '\'' or '"')
            {
                bool escape = c == '\'' && (!standardStrings ||
                    i > 0 && sql[i - 1] is 'e' or 'E' &&
                    (i < 2 || !char.IsLetterOrDigit(sql[i - 2]) && sql[i - 2] is not ('_' or '$') && sql[i - 2] < 128));
                i++;
                while (i < sql.Length)
                {
                    if (escape && sql[i] == '\\') { i = Math.Min(i + 2, sql.Length); continue; }
                    if (sql[i++] != c) continue;
                    if (i < sql.Length && sql[i] == c) { i++; continue; }
                    break;
                }
                start = false; continue;
            }
            if (c == '$')
            {
                int end = i + 1;
                while (end < sql.Length && (char.IsLetterOrDigit(sql[end]) || sql[end] == '_' || sql[end] >= 128)) end++;
                if (end < sql.Length && sql[end] == '$' && (end == i + 1 || !char.IsAsciiDigit(sql[i + 1])))
                {
                    string delimiter = sql[i..(end + 1)];
                    int close = sql.IndexOf(delimiter, end + 1, StringComparison.Ordinal);
                    i = close < 0 ? sql.Length : close + delimiter.Length;
                    start = false; continue;
                }
            }
            if (char.IsLetter(c) || c == '_' || c >= 128)
            {
                int end = i + 1;
                while (end < sql.Length && (char.IsLetterOrDigit(sql[end]) || sql[end] is '_' or '$' || sql[end] >= 128)) end++;
                if (!start) { i = end; continue; }
                string word = sql[i..end].ToUpperInvariant();
                int next = end;
                string following = NextControlWord(sql, ref next);
                bool rollbackToSavepoint = word == "ROLLBACK" &&
                    (following == "TO" || following is "WORK" or "TRANSACTION" && NextControlWord(sql, ref next) == "TO");
                if (word is "BEGIN" or "START" or "COMMIT" or "END" or "ABORT" ||
                    word == "ROLLBACK" && !rollbackToSavepoint || word == "PREPARE" && following == "TRANSACTION")
                    throw new InvalidOperationException("Transaction-control SQL is owned by the scoped token. Use Rollback() to roll back.");
                i = end; start = false; continue;
            }
            start = false; i++;
        }
    }
    private static string NextControlWord(string sql, ref int offset)
    {
        while (offset < sql.Length)
        {
            if (char.IsWhiteSpace(sql[offset])) { offset++; continue; }
            if (offset + 1 < sql.Length && sql[offset] == '-' && sql[offset + 1] == '-')
            { offset += 2; while (offset < sql.Length && sql[offset] is not ('\r' or '\n')) offset++; continue; }
            if (offset + 1 < sql.Length && sql[offset] == '/' && sql[offset + 1] == '*')
            {
                offset += 2; int depth = 1;
                while (offset < sql.Length && depth != 0)
                {
                    if (offset + 1 < sql.Length && sql[offset] == '/' && sql[offset + 1] == '*') { depth++; offset += 2; }
                    else if (offset + 1 < sql.Length && sql[offset] == '*' && sql[offset + 1] == '/') { depth--; offset += 2; }
                    else offset++;
                }
                continue;
            }
            int start = offset;
            while (offset < sql.Length && (char.IsLetterOrDigit(sql[offset]) || sql[offset] is '_' or '$' || sql[offset] >= 128)) offset++;
            return sql[start..offset].ToUpperInvariant();
        }
        return "";
    }
    public void Rollback()
        => database.WithTransaction(this, () => { database.Query("ROLLBACK").EnsureSuccess(); Complete(); return 0; });
}

public sealed unsafe partial class PgliteDatabase
{
    public T Transaction<T>(Func<PgliteTransaction, T> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        RejectAsyncServiceCallback<T>(callback);
        RequireServiceIdle(); BeforeQuery();
        var listenersBefore = serviceListeners.ToDictionary(pair => pair.Key,
            pair => new HashSet<Action<string>>(pair.Value), StringComparer.Ordinal);
        var token = new PgliteTransaction(this);
        serviceBatchDepth++;
        T result;
        try
        {
            Query("BEGIN").EnsureSuccess();
            serviceTransaction = token;
            try
            {
                result = callback(token);
                if (!token.Closed)
                {
                    // A caller catching a SQL error must explicitly roll back;
                    // PostgreSQL otherwise reports COMMIT as ROLLBACK on an aborted block.
                    if (serviceTransactionStatus == PgliteTransactionStatus.FailedTransaction)
                        throw new InvalidOperationException("The transaction failed; it cannot commit.");
                    WithTransaction(token, () => Query("COMMIT").EnsureSuccess());
                }
                else RestoreServiceListeners(listenersBefore);
            }
            catch (Exception original)
            {
                if (!token.Closed && Ready)
                {
                    try { WithTransaction(token, () => Query("ROLLBACK").EnsureSuccess()); }
                    catch (Exception rollbackError)
                    { throw new AggregateException("Transaction callback and rollback both failed.", original, rollbackError); }
                }
                RestoreServiceListeners(listenersBefore);
                throw;
            }
            finally { token.Complete(); serviceTransaction = null; serviceTransactionEntry = false; }
        }
        finally
        {
            token.Complete(); serviceTransaction = null; serviceTransactionEntry = false; serviceBatchDepth--;
            // Do not run callbacks from a finally: preserve the original callback/SQL
            // error. Pending notices are drained by the next successful public operation.
        }
        DrainServiceResponses();
        return result;
    }

    public void Transaction(Action<PgliteTransaction> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        RejectAsyncVoidServiceCallback(callback);
        Transaction(tx => { callback(tx); return 0; });
        DrainServiceResponses();
    }

    internal T WithTransaction<T>(PgliteTransaction token, Func<T> callback)
    {
        RequireOwner(); token.RequireOpen();
        if (!ReferenceEquals(token, serviceTransaction)) throw new InvalidOperationException("This transaction does not own the active scope.");
        if (serviceTransactionEntry || serviceDeliveringCallbacks)
            throw new InvalidOperationException("Concurrent or recursive transaction-token entry is forbidden.");
        serviceTransactionEntry = true;
        try { return callback(); }
        finally { serviceTransactionEntry = false; }
    }

    private void RestoreServiceListeners(Dictionary<string, HashSet<Action<string>>> before)
    {
        serviceListeners.Clear();
        foreach (var pair in before) serviceListeners.Add(pair.Key, pair.Value);
    }
}

using Managed.PGlite;

int failures = 0;
var recovery = new PgliteDatabase();
var recoveryToken = recovery.ReviewToken(PgliteTransactionStatus.FailedTransaction);
try { recoveryToken.Exec("ROLLBACK TO SAVEPOINT recover_here"); }
catch (PgliteSqlException error) { Console.WriteLine("F1 observed preflight error: " + error.Result.Errors[0].SqlState); }
Check(recovery.Calls.Contains("ROLLBACK TO SAVEPOINT recover_here"), "F1 recovery command reaches engine without an intervening SHOW");

var escape = new PgliteDatabase();
var escapeToken = escape.ReviewToken(PgliteTransactionStatus.InTransaction);
const string escapingSql = "INSERT INTO audit_escape$tag$ VALUES (1); COMMIT";
bool rejected = false;
try { escapeToken.Exec(escapingSql); }
catch (InvalidOperationException) { rejected = true; }
Check(rejected && !escape.Calls.Contains(escapingSql), "F2 identifier dollar signs cannot hide outer COMMIT");

const string crSql = "SELECT $1::int AS a --comment\r, $2::int AS b";
string formatted = PgliteDatabase.ReviewSubstitute(crSql, ["1", "2"], true);
Check(formatted == "SELECT 1::int AS a --comment\r, 2::int AS b", "F3 CR ends line comment for parameter formatting");
Console.WriteLine("Observed regression failures: " + failures);
return failures == 0 ? 0 : 1;

void Check(bool condition, string name)
{
    Console.WriteLine((condition ? "PASS " : "FAIL ") + name);
    if (!condition) failures++;
}

namespace Managed.PGlite
{
    // Deliberately small response/ownership seam. The actual validator and formatter
    // are compiled from src/Api. This is not engine qualification. The matching
    // PostgreSQL parsing/25P02 behavior was independently checked with pinned npm.
    public sealed unsafe partial class PgliteDatabase
    {
        private readonly Dictionary<string, HashSet<Action<string>>> serviceListeners = [];
        private PgliteTransaction? serviceTransaction;
        private bool serviceTransactionEntry, serviceDeliveringCallbacks;
        private int serviceBatchDepth;
        private PgliteTransactionStatus serviceTransactionStatus;
        public bool Ready => true;
        public List<string> Calls { get; } = [];
        public PgliteTransaction ReviewToken(PgliteTransactionStatus status)
        { serviceTransactionStatus = status; return serviceTransaction = new(this); }
        public static string ReviewSubstitute(string sql, string[] values, bool standard)
            => SubstituteParameters(sql, values, standard);
        internal bool UsesStandardStrings() => true;
        private void RequireOwner() { }
        private void RequireServiceIdle() { }
        private void BeforeQuery() { }
        private void RequireQueryEntry() { }
        private void DrainServiceResponses() { }
        private static void RejectAsyncServiceCallback<T>(Delegate callback) { }
        private static void RejectAsyncVoidServiceCallback(Delegate callback) { }
        public PgliteQueryDescription DescribeQuery(string sql, IReadOnlyList<uint>? types = null) => new([23, 23], []);
        private PgliteQueryResult QueryCore(string sql) => Query(sql);
        public PgliteQueryResult Query(string sql)
        {
            Calls.Add(sql);
            if (sql == "SHOW standard_conforming_strings" && serviceTransactionStatus == PgliteTransactionStatus.FailedTransaction)
                return new([], [new(new() { ['C'] = "25P02", ['S'] = "ERROR", ['M'] = "current transaction is aborted" })], [], new(), serviceTransactionStatus);
            return new([new([], [new string?[] { "on" }], "SELECT 1")], [], [], new(), serviceTransactionStatus);
        }
        public PgliteQueryResult Query(string sql, IReadOnlyList<object?> values, PgliteQueryOptions? options = null) => Query(sql);
        public PgliteQueryResult Exec(string sql) => Query(sql).EnsureSuccess();
        public PgliteQueryResult Sql(PgliteSql sql) => Query(sql.Text);
        public PgliteSubscription Listen(string channel, Action<string> callback) => new();
        public void Unlisten(string channel, Action<string>? callback = null) { }
    }
    public sealed class PgliteSubscription { }
}

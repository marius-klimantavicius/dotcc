using System.Collections.ObjectModel;

namespace Managed.PGlite;

/// <summary>The transaction state reported by PostgreSQL's ReadyForQuery frame.</summary>
public enum PgliteTransactionStatus : byte
{
    Idle = (byte)'I', InTransaction = (byte)'T', FailedTransaction = (byte)'E'
}

public sealed record PgliteColumn(string Name, uint TableOid, short AttributeNumber,
    uint TypeOid, short TypeLength, int TypeModifier);

/// <summary>Owned managed copies of one original PostgreSQL result set.</summary>
public sealed class PgliteResultSet
{
    public IReadOnlyList<PgliteColumn> Columns { get; }
    public IReadOnlyList<IReadOnlyList<string?>> Rows { get; }
    public string Command { get; }

    internal PgliteResultSet(List<PgliteColumn> columns, List<IReadOnlyList<string?>> rows, string command)
    { Columns = columns.AsReadOnly(); Rows = rows.AsReadOnly(); Command = command; }
}

public sealed class PgliteError
{
    public IReadOnlyDictionary<char, string> Fields { get; }
    public string SqlState => Fields['C'];
    public string Message => Fields['M'];
    public string Severity => Fields.TryGetValue('V', out string? value) ? value : Fields['S'];
    internal PgliteError(Dictionary<char, string> fields)
    { Fields = new ReadOnlyDictionary<char, string>(fields); }
    public override string ToString() => $"{Severity} {SqlState}: {Message}";
}

/// <summary>SQL errors are returned with the complete response and transaction
/// status after real PostgreSQL recovery. Host/protocol failures throw.</summary>
public sealed class PgliteQueryResult
{
    public IReadOnlyList<PgliteResultSet> ResultSets { get; }
    public IReadOnlyList<PgliteError> Errors { get; }
    public IReadOnlyList<PgliteError> Notices { get; }
    public IReadOnlyDictionary<string, string> Parameters { get; }
    public PgliteTransactionStatus TransactionStatus { get; }
    public bool IsSuccess => Errors.Count == 0;

    internal PgliteQueryResult(List<PgliteResultSet> sets, List<PgliteError> errors,
        List<PgliteError> notices, Dictionary<string, string> parameters, PgliteTransactionStatus status)
    {
        ResultSets = sets.AsReadOnly(); Errors = errors.AsReadOnly(); Notices = notices.AsReadOnly();
        Parameters = new ReadOnlyDictionary<string, string>(parameters); TransactionStatus = status;
    }

    public PgliteQueryResult EnsureSuccess()
    { if (!IsSuccess) throw new PgliteSqlException(this); return this; }
}

public sealed class PgliteSqlException : Exception
{
    public PgliteQueryResult Result { get; }
    internal PgliteSqlException(PgliteQueryResult result) : base(result.Errors[0].ToString()) { Result = result; }
}

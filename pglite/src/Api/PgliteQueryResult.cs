using System.Collections.ObjectModel;
using System.Globalization;

namespace Managed.PGlite;

/// <summary>The transaction state reported by PostgreSQL's ReadyForQuery frame.</summary>
public enum PgliteTransactionStatus : byte
{
    Idle = (byte)'I', InTransaction = (byte)'T', FailedTransaction = (byte)'E'
}

public sealed record PgliteColumn(string Name, uint TableOid, short AttributeNumber,
    uint TypeOid, short TypeLength, int TypeModifier, short FormatCode = 0);

public sealed record PgliteNotification(int ProcessId, string Channel, string Payload);

/// <summary>Parameter OIDs and result columns reported by the server, without
/// executing the described statement.</summary>
public sealed record PgliteQueryDescription(IReadOnlyList<uint> ParameterTypes,
    IReadOnlyList<PgliteColumn> Columns)
{
    public IReadOnlyList<PgliteColumn> ResultFields => Columns;
}

public sealed record PgliteCopyResponse(char Direction, byte Format,
    IReadOnlyList<short> ColumnFormats);

/// <summary>A decoded backend message with an owned copy of its complete wire
/// frame. Value contains the decoded payload; Data includes type and length.</summary>
public sealed record PgliteBackendMessage(char Type, string Name,
    ReadOnlyMemory<byte> Data, object? Value);

/// <summary>Owned managed copies of one original PostgreSQL result set.</summary>
public sealed class PgliteResultSet
{
    public IReadOnlyList<PgliteColumn> Columns { get; }
    public IReadOnlyList<IReadOnlyList<string?>> Rows { get; }
    public IReadOnlyList<IReadOnlyList<object?>> TypedRows { get; }
    public IReadOnlyList<IReadOnlyDictionary<string, object?>> TypedObjectRows { get; }
    /// <summary>Binary result values, when requested through the raw protocol.
    /// Text fields retain their UTF-8 bytes; SQL NULL is represented by null.</summary>
    public IReadOnlyList<IReadOnlyList<byte[]?>> RawRows { get; }
    public string Command { get; }
    /// <summary>First word of the PostgreSQL command tag, when present.</summary>
    public string? CommandName { get; }
    /// <summary>Per-statement count from the command tag, including SELECT.</summary>
    public long? RowCount { get; }
    /// <summary>Cumulative INSERT/UPDATE/DELETE/COPY/MERGE count in this response.</summary>
    public long AffectedRows { get; }

    internal PgliteResultSet(List<PgliteColumn> columns, List<IReadOnlyList<string?>> rows, string command,
        List<IReadOnlyList<object?>>? typedRows = null, List<IReadOnlyList<byte[]?>>? rawRows = null,
        long affectedRows = 0)
    {
        Columns = columns.AsReadOnly(); Rows = rows.AsReadOnly(); Command = command;
        if (command.Length != 0)
        {
            int firstSpace = command.IndexOf(' ');
            CommandName = firstSpace < 0 ? command : command[..firstSpace];
            if (long.TryParse(command.AsSpan(command.LastIndexOf(' ') + 1), NumberStyles.None,
                CultureInfo.InvariantCulture, out long count)) RowCount = count;
        }
        AffectedRows = checked(affectedRows + (CommandName is "INSERT" or "UPDATE" or "DELETE" or "COPY" or "MERGE" ? RowCount ?? 0 : 0));
        TypedRows = (typedRows ?? rows.Select(row => (IReadOnlyList<object?>)Array.AsReadOnly(row.Cast<object?>().ToArray())).ToList()).AsReadOnly();
        RawRows = (rawRows ?? new()).AsReadOnly();
        TypedObjectRows = TypedRows.Select(row =>
        {
            var fields = new Dictionary<string, object?>(StringComparer.Ordinal);
            for (int i = 0; i < columns.Count; i++) fields[columns[i].Name] = row[i];
            return (IReadOnlyDictionary<string, object?>)new ReadOnlyDictionary<string, object?>(fields);
        }).ToList().AsReadOnly();
    }
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
    public IReadOnlyList<PgliteNotification> Notifications { get; }
    public IReadOnlyList<PgliteBackendMessage> Messages { get; }
    public IReadOnlyList<uint> ParameterTypes { get; }
    public IReadOnlyList<PgliteColumn> DescribedColumns { get; }
    public IReadOnlyList<PgliteCopyResponse> CopyResponses { get; }
    public IReadOnlyList<byte[]> CopyData { get; }
    public bool HasReadyForQuery { get; }
    public bool IsComplete => HasReadyForQuery;
    public IReadOnlyDictionary<string, string> Parameters { get; }
    public PgliteTransactionStatus TransactionStatus { get; }
    public bool IsSuccess => Errors.Count == 0;
    public long AffectedRows => ResultSets.Count == 0 ? 0 : ResultSets[^1].AffectedRows;

    internal PgliteQueryResult(List<PgliteResultSet> sets, List<PgliteError> errors,
        List<PgliteError> notices, Dictionary<string, string> parameters, PgliteTransactionStatus status,
        List<PgliteNotification>? notifications = null, List<PgliteBackendMessage>? messages = null,
        List<uint>? parameterTypes = null, List<PgliteColumn>? describedColumns = null,
        List<PgliteCopyResponse>? copyResponses = null, List<byte[]>? copyData = null, bool hasReadyForQuery = true)
    {
        ResultSets = sets.AsReadOnly(); Errors = errors.AsReadOnly(); Notices = notices.AsReadOnly();
        Parameters = new ReadOnlyDictionary<string, string>(parameters); TransactionStatus = status;
        Notifications = (notifications ?? new()).AsReadOnly(); Messages = (messages ?? new()).AsReadOnly();
        ParameterTypes = (parameterTypes ?? new()).AsReadOnly(); DescribedColumns = (describedColumns ?? new()).AsReadOnly();
        CopyResponses = (copyResponses ?? new()).AsReadOnly(); CopyData = (copyData ?? new()).AsReadOnly();
        HasReadyForQuery = hasReadyForQuery;
    }

    public PgliteQueryResult EnsureSuccess()
    { if (!IsSuccess) throw new PgliteSqlException(this); return this; }
}

public sealed class PgliteSqlException : Exception
{
    public PgliteQueryResult Result { get; }
    internal PgliteSqlException(PgliteQueryResult result) : base(result.Errors[0].ToString()) { Result = result; }
}

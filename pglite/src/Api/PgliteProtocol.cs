using System.Buffers.Binary;
using System.Text;

namespace Managed.PGlite;

/// <summary>PostgreSQL v3 Simple Query transport. All numeric fields use network
/// byte order. Decoded values own their storage and never borrow core memory.</summary>
public static class PgliteProtocol
{
    private static readonly UTF8Encoding utf8 = new(false, true);

    public static byte[] EncodeQuery(string sql)
    {
        ArgumentNullException.ThrowIfNull(sql);
        if (sql.Contains('\0')) throw new ArgumentException("SQL cannot contain NUL.", nameof(sql));
        int count = utf8.GetByteCount(sql);
        byte[] frame = new byte[checked(count + 6)];
        frame[0] = (byte)'Q';
        BinaryPrimitives.WriteInt32BigEndian(frame.AsSpan(1), checked(count + 5));
        utf8.GetBytes(sql, frame.AsSpan(5, count));
        return frame;
    }

    public static byte[] EncodeTerminate() => [(byte)'X', 0, 0, 0, 4];

    /// <summary>Decode one complete response ending in exactly one ReadyForQuery.
    /// Rejects invalid lengths, UTF-8, row shapes, error fields and frame order.</summary>
    public static PgliteQueryResult DecodeResponse(ReadOnlySpan<byte> response)
    {
        var sets = new List<PgliteResultSet>();
        var errors = new List<PgliteError>();
        var notices = new List<PgliteError>();
        var parameters = new Dictionary<string, string>(StringComparer.Ordinal);
        List<PgliteColumn>? columns = null;
        var rows = new List<IReadOnlyList<string?>>();
        bool ready = false;
        var status = PgliteTransactionStatus.Idle;
        var frames = new Reader(response);
        while (!frames.End)
        {
            if (ready) throw Invalid("Data after ReadyForQuery.");
            byte type = frames.Byte();
            int length = frames.Int32();
            if (length < 4) throw Invalid("Invalid message length.");
            var body = new Reader(frames.Bytes(length - 4));
            switch (type)
            {
                case (byte)'T':
                    if (columns != null) throw Invalid("Overlapping row descriptions.");
                    columns = new();
                    int count = body.Int16();
                    if (count < 0 || count > body.Remaining / 19) throw Invalid("Invalid column count.");
                    for (int i = 0; i < count; i++)
                    {
                        string name = body.String();
                        uint table = body.UInt32(); short attribute = body.Int16();
                        uint oid = body.UInt32(); short size = body.Int16(); int modifier = body.Int32();
                        if (body.Int16() != 0) throw Invalid("Simple Query requires text result columns.");
                        columns.Add(new(name, table, attribute, oid, size, modifier));
                    }
                    break;
                case (byte)'D':
                    int fields = body.Int16();
                    if (columns == null || fields != columns.Count) throw Invalid("DataRow does not match RowDescription.");
                    var row = new string?[fields];
                    for (int i = 0; i < fields; i++)
                    {
                        int size = body.Int32();
                        if (size < -1) throw Invalid("Invalid field length.");
                        if (size >= 0) row[i] = Text(body.Bytes(size));
                    }
                    rows.Add(Array.AsReadOnly(row));
                    break;
                case (byte)'C':
                    string command = body.String();
                    if (command.Length == 0) throw Invalid("Empty command completion.");
                    sets.Add(new(columns ?? new(), rows, command));
                    columns = null; rows = new();
                    break;
                case (byte)'I':
                    if (columns != null) throw Invalid("Empty query inside a result set.");
                    sets.Add(new(new(), new(), ""));
                    break;
                case (byte)'E':
                    errors.Add(Error(ref body));
                    // An execution error may interrupt an already described
                    // result. Retain any rows actually received before error.
                    if (columns != null) sets.Add(new(columns, rows, ""));
                    columns = null; rows = new();
                    break;
                case (byte)'N': notices.Add(Error(ref body)); break;
                case (byte)'S': parameters[body.String()] = body.String(); break;
                case (byte)'Z':
                    if (columns != null) throw Invalid("Unfinished result before ReadyForQuery.");
                    status = (PgliteTransactionStatus)body.Byte();
                    if (status is not (PgliteTransactionStatus.Idle or PgliteTransactionStatus.InTransaction or PgliteTransactionStatus.FailedTransaction))
                        throw Invalid("Invalid transaction status.");
                    ready = true;
                    break;
                default: throw Invalid($"Unsupported backend message {(char)type}.");
            }
            if (!body.End) throw Invalid("Trailing message payload.");
        }
        if (!ready) throw Invalid("Missing ReadyForQuery.");
        return new(sets, errors, notices, parameters, status);
    }

    private static PgliteError Error(ref Reader body)
    {
        var fields = new Dictionary<char, string>();
        byte field;
        while ((field = body.Byte()) != 0)
        {
            if (field is < 32 or > 126 || !fields.TryAdd((char)field, body.String()))
                throw Invalid("Invalid or duplicate error field.");
        }
        if (!fields.TryGetValue('C', out string? code) || code.Length != 5 ||
            code.Any(c => c is not (>= '0' and <= '9') and not (>= 'A' and <= 'Z')) ||
            !fields.ContainsKey('M') || !fields.ContainsKey('S'))
            throw Invalid("Incomplete PostgreSQL error response.");
        return new(fields);
    }

    private static string Text(ReadOnlySpan<byte> bytes)
    {
        try { return utf8.GetString(bytes); }
        catch (DecoderFallbackException error) { throw new InvalidDataException("Invalid UTF-8 in PostgreSQL response.", error); }
    }
    private static InvalidDataException Invalid(string message) => new("PostgreSQL protocol: " + message);

    private ref struct Reader(ReadOnlySpan<byte> bytes)
    {
        private ReadOnlySpan<byte> remaining = bytes;
        internal readonly bool End => remaining.IsEmpty;
        internal readonly int Remaining => remaining.Length;
        internal ReadOnlySpan<byte> Bytes(int count)
        {
            if (count < 0 || count > remaining.Length) throw Invalid("Truncated message.");
            var result = remaining[..count]; remaining = remaining[count..]; return result;
        }
        internal byte Byte() => Bytes(1)[0];
        internal short Int16() => BinaryPrimitives.ReadInt16BigEndian(Bytes(2));
        internal int Int32() => BinaryPrimitives.ReadInt32BigEndian(Bytes(4));
        internal uint UInt32() => BinaryPrimitives.ReadUInt32BigEndian(Bytes(4));
        internal string String()
        {
            int end = remaining.IndexOf((byte)0);
            if (end < 0) throw Invalid("Unterminated string.");
            string value = Text(Bytes(end)); Bytes(1); return value;
        }
    }
}

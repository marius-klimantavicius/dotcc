// Manual PostgreSQL v3 protocol port from @electric-sql/pg-protocol.
// Package metadata declares Apache-2.0; the retained MIT license and Brian
// Carlson attribution are delivered in Notices/PG-PROTOCOL-MIT.txt.
using System.Buffers.Binary;
using System.Text;

namespace Managed.PGlite;

/// <summary>PostgreSQL v3 wire messages. Numeric fields use network byte order;
/// returned buffers and decoded values own their storage.</summary>
/// <remarks>Network startup, SSL, password/SASL and CancelRequest serializers
/// are outside the embedded API: Create/Open initialize and attach the translated
/// backend directly. Authentication and version-negotiation responses are still
/// preserved as owned payload bytes by the generic decoder.</remarks>
public static class PgliteProtocol
{
    internal static readonly UTF8Encoding Utf8 = new(false, true);

    public static byte[] EncodeQuery(string sql) => CStringFrame('Q', sql);
    public static byte[] EncodeTerminate() => Frame('X', []);
    public static byte[] EncodeSync() => Frame('S', []);
    public static byte[] EncodeFlush() => Frame('H', []);
    public static byte[] EncodeCopyDone() => Frame('c', []);
    public static byte[] EncodeCopyData(ReadOnlySpan<byte> data) => Frame('d', data);
    public static byte[] EncodeCopyFail(string message) => CStringFrame('f', message);

    public static byte[] EncodeParse(string sql, IReadOnlyList<uint>? parameterTypes = null, string statement = "")
    {
        using var body = new MemoryStream();
        CString(body, statement); CString(body, sql);
        Count(body, parameterTypes?.Count ?? 0);
        if (parameterTypes != null) foreach (uint oid in parameterTypes) Int32(body, unchecked((int)oid));
        return Frame('P', body.ToArray());
    }

    /// <summary>Bind nullable text parameters and request text result columns.
    /// SQL NULL has wire length -1, distinct from an empty string.</summary>
    public static byte[] EncodeBind(IReadOnlyList<string?> values, string portal = "", string statement = "")
    {
        ArgumentNullException.ThrowIfNull(values);
        using var body = new MemoryStream();
        CString(body, portal); CString(body, statement);
        Count(body, 0); // all parameters use text format
        Count(body, values.Count);
        foreach (string? value in values)
        {
            if (value == null) Int32(body, -1);
            else
            {
                byte[] bytes = Utf8.GetBytes(value);
                Int32(body, bytes.Length); body.Write(bytes);
            }
        }
        Count(body, 0); // all result columns use text format
        return Frame('B', body.ToArray());
    }

    /// <summary>Bind wire values with explicit per-parameter/result formats.
    /// Format codes are 0 (text) and 1 (binary).</summary>
    public static byte[] EncodeBindRaw(IReadOnlyList<byte[]?> values,
        IReadOnlyList<short>? parameterFormats = null, IReadOnlyList<short>? resultFormats = null,
        string portal = "", string statement = "")
    {
        ArgumentNullException.ThrowIfNull(values);
        if (parameterFormats != null && parameterFormats.Count != 0 && parameterFormats.Count != 1 && parameterFormats.Count != values.Count)
            throw new ArgumentException("Parameter formats must contain zero, one, or one format per value.", nameof(parameterFormats));
        using var body = new MemoryStream();
        CString(body, portal); CString(body, statement);
        Formats(body, parameterFormats); Count(body, values.Count);
        foreach (byte[]? value in values)
        {
            Int32(body, value?.Length ?? -1);
            if (value != null) body.Write(value);
        }
        Formats(body, resultFormats);
        return Frame('B', body.ToArray());
    }

    public static byte[] EncodeDescribe(char type = 'S', string name = "") => TargetFrame('D', type, name);
    public static byte[] EncodeClose(char type = 'S', string name = "") => TargetFrame('C', type, name);

    public static byte[] EncodeExecute(string portal = "", int maximumRows = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maximumRows);
        using var body = new MemoryStream();
        CString(body, portal); Int32(body, maximumRows);
        return Frame('E', body.ToArray());
    }

    public static byte[] Concat(params byte[][] frames)
    {
        ArgumentNullException.ThrowIfNull(frames);
        using var bytes = new MemoryStream();
        foreach (byte[] frame in frames) { ArgumentNullException.ThrowIfNull(frame); bytes.Write(frame); }
        return bytes.ToArray();
    }

    /// <summary>Decode a complete response with exactly one ReadyForQuery.
    /// Use PgliteProtocolDecoder for fragmented or partial extended responses.</summary>
    public static PgliteQueryResult DecodeResponse(ReadOnlySpan<byte> response,
        Func<string?, uint, object?>? decode = null)
        => new PgliteProtocolDecoder().DecodeCore(response, decode, requireReady: true);

    internal static byte[] Frame(char type, ReadOnlySpan<byte> body)
    {
        byte[] frame = new byte[checked(body.Length + 5)];
        frame[0] = checked((byte)type);
        BinaryPrimitives.WriteInt32BigEndian(frame.AsSpan(1), checked(body.Length + 4));
        body.CopyTo(frame.AsSpan(5));
        return frame;
    }

    private static byte[] CStringFrame(char type, string text)
    {
        using var body = new MemoryStream(); CString(body, text); return Frame(type, body.ToArray());
    }
    private static byte[] TargetFrame(char code, char type, string name)
    {
        if (type is not ('S' or 'P')) throw new ArgumentOutOfRangeException(nameof(type));
        using var body = new MemoryStream(); body.WriteByte((byte)type); CString(body, name); return Frame(code, body.ToArray());
    }
    private static void CString(Stream stream, string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text.Contains('\0')) throw new ArgumentException("Protocol strings cannot contain NUL.", nameof(text));
        stream.Write(Utf8.GetBytes(text)); stream.WriteByte(0);
    }
    private static void Count(Stream stream, int value)
    {
        if ((uint)value > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(value), "The protocol supports at most 65535 items.");
        Span<byte> bytes = stackalloc byte[2]; BinaryPrimitives.WriteUInt16BigEndian(bytes, (ushort)value); stream.Write(bytes);
    }
    private static void Int32(Stream stream, int value)
    {
        Span<byte> bytes = stackalloc byte[4]; BinaryPrimitives.WriteInt32BigEndian(bytes, value); stream.Write(bytes);
    }
    private static void Formats(Stream stream, IReadOnlyList<short>? formats)
    {
        Count(stream, formats?.Count ?? 0);
        if (formats == null) return;
        foreach (short format in formats)
        {
            if (format is not (0 or 1)) throw new ArgumentOutOfRangeException(nameof(formats));
            Count(stream, format);
        }
    }

    internal static string Text(ReadOnlySpan<byte> bytes)
    {
        try { return Utf8.GetString(bytes); }
        catch (DecoderFallbackException error) { throw new InvalidDataException("Invalid UTF-8 in PostgreSQL response.", error); }
    }
    internal static InvalidDataException Invalid(string message) => new("PostgreSQL protocol: " + message);

    internal ref struct Reader(ReadOnlySpan<byte> bytes)
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
        internal ushort UInt16() => BinaryPrimitives.ReadUInt16BigEndian(Bytes(2));
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

/// <summary>An incremental backend decoder. Retains incomplete frame bytes and
/// row descriptions across calls, as required by separate Describe/Execute
/// exchanges. An absent ReadyForQuery means the protocol exchange is partial;
/// TransactionStatus then retains the last state actually reported by PostgreSQL.</summary>
public sealed class PgliteProtocolDecoder
{
    private byte[] pending = [];
    private List<PgliteColumn>? columns;
    private List<IReadOnlyList<string?>> rows = new();
    private List<IReadOnlyList<object?>> typedRows = new();
    private List<IReadOnlyList<byte[]?>> rawRows = new();
    private bool statementDescription;
    private bool descriptionOnly;
    private PgliteTransactionStatus status = PgliteTransactionStatus.Idle;

    public PgliteQueryResult Decode(ReadOnlySpan<byte> response, Func<string?, uint, object?>? decode = null)
        => DecodeCore(response, decode, requireReady: false);

    public void Reset()
    {
        pending = []; ClearRows(); statementDescription = false; status = PgliteTransactionStatus.Idle;
    }

    internal PgliteQueryResult DecodeCore(ReadOnlySpan<byte> response,
        Func<string?, uint, object?>? decode, bool requireReady)
    {
        byte[] bytes;
        if (pending.Length == 0) bytes = response.ToArray();
        else { bytes = new byte[checked(pending.Length + response.Length)]; pending.CopyTo(bytes, 0); response.CopyTo(bytes.AsSpan(pending.Length)); }
        pending = [];
        var sets = new List<PgliteResultSet>();
        var errors = new List<PgliteError>();
        var notices = new List<PgliteError>();
        var notifications = new List<PgliteNotification>();
        var messages = new List<PgliteBackendMessage>();
        var parameters = new Dictionary<string, string>(StringComparer.Ordinal);
        var parameterTypes = new List<uint>();
        var describedColumns = new List<PgliteColumn>();
        var copyResponses = new List<PgliteCopyResponse>();
        var copyData = new List<byte[]>();
        bool ready = false;
        long affectedRows = 0;
        int offset = 0;
        while (offset < bytes.Length)
        {
            if (requireReady && ready) throw PgliteProtocol.Invalid("Data after ReadyForQuery.");
            if (bytes.Length - offset < 5) break;
            char type = (char)bytes[offset];
            int length = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(offset + 1));
            if (length < 4 || length == int.MaxValue) throw PgliteProtocol.Invalid("Invalid message length.");
            if (length > bytes.Length - offset - 1) break;
            byte[] frame = bytes.AsSpan(offset, length + 1).ToArray();
            var body = new PgliteProtocol.Reader(frame.AsSpan(5));
            object? value = null;
            string name;
            switch (type)
            {
                case 'T':
                    name = "rowDescription";
                    if (columns != null && rows.Count != 0) throw PgliteProtocol.Invalid("Overlapping row descriptions.");
                    columns = new();
                    int count = body.UInt16();
                    if (count > body.Remaining / 19) throw PgliteProtocol.Invalid("Invalid column count.");
                    for (int i = 0; i < count; i++)
                    {
                        string fieldName = body.String();
                        uint table = body.UInt32(); short attribute = body.Int16();
                        uint oid = body.UInt32(); short size = body.Int16(); int modifier = body.Int32(); short format = body.Int16();
                        if (format is not (0 or 1)) throw PgliteProtocol.Invalid("Invalid result format.");
                        columns.Add(new(fieldName, table, attribute, oid, size, modifier, format));
                    }
                    descriptionOnly = statementDescription; statementDescription = false;
                    describedColumns = new(columns); value = columns.AsReadOnly();
                    break;
                case 'D':
                    name = "dataRow";
                    int fields = body.UInt16();
                    if (columns == null || fields != columns.Count) throw PgliteProtocol.Invalid("DataRow does not match RowDescription.");
                    var row = new string?[fields]; var typed = new object?[fields]; var raw = new byte[]?[fields];
                    for (int i = 0; i < fields; i++)
                    {
                        int size = body.Int32();
                        if (size < -1) throw PgliteProtocol.Invalid("Invalid field length.");
                        if (size >= 0)
                        {
                            raw[i] = body.Bytes(size).ToArray();
                            if (columns[i].FormatCode == 0)
                            {
                                row[i] = PgliteProtocol.Text(raw[i]!);
                                typed[i] = decode == null ? row[i] : decode(row[i], columns[i].TypeOid);
                            }
                            else typed[i] = raw[i];
                        }
                    }
                    rows.Add(Array.AsReadOnly(row)); typedRows.Add(Array.AsReadOnly(typed)); rawRows.Add(Array.AsReadOnly(raw));
                    descriptionOnly = false; value = Array.AsReadOnly(typed);
                    break;
                case 'C':
                    name = "commandComplete"; string command = body.String();
                    if (command.Length == 0) throw PgliteProtocol.Invalid("Empty command completion.");
                    var completed = new PgliteResultSet(columns ?? new(), rows, command, typedRows, rawRows, affectedRows);
                    sets.Add(completed); affectedRows = completed.AffectedRows;
                    value = command; ClearRows();
                    break;
                case 's':
                    name = "portalSuspended";
                    if (columns != null) sets.Add(new(new(columns), rows, "", typedRows, rawRows, affectedRows));
                    rows = new(); typedRows = new(); rawRows = new(); descriptionOnly = true;
                    break;
                case 'I':
                    name = "emptyQuery";
                    if (columns != null && !descriptionOnly) throw PgliteProtocol.Invalid("Empty query inside a result set.");
                    sets.Add(new(new(), new(), "", affectedRows: affectedRows)); ClearRows();
                    break;
                case 'E':
                    name = "error"; var error = Error(ref body); errors.Add(error); value = error;
                    if (columns != null && !descriptionOnly) sets.Add(new(columns, rows, "", typedRows, rawRows, affectedRows));
                    ClearRows(); statementDescription = false;
                    break;
                case 'N': name = "notice"; var notice = Error(ref body); notices.Add(notice); value = notice; break;
                case 'A':
                    name = "notification";
                    var notification = new PgliteNotification(body.Int32(), body.String(), body.String());
                    notifications.Add(notification); value = notification;
                    break;
                case 'S':
                    name = "parameterStatus"; string key = body.String(); string setting = body.String();
                    parameters[key] = setting; value = new KeyValuePair<string, string>(key, setting);
                    break;
                case 't':
                    name = "parameterDescription"; int parameterCount = body.UInt16();
                    if (parameterCount > body.Remaining / 4) throw PgliteProtocol.Invalid("Invalid parameter count.");
                    for (int i = 0; i < parameterCount; i++) parameterTypes.Add(body.UInt32());
                    statementDescription = true; value = parameterTypes.AsReadOnly();
                    break;
                case 'n': name = "noData"; ClearRows(); statementDescription = false; break;
                case '1': name = "parseComplete"; break;
                case '2': name = "bindComplete"; break;
                case '3': name = "closeComplete"; break;
                case 'W' when body.End: name = "replicationStart"; break;
                case 'G': case 'H': case 'W':
                    name = type == 'G' ? "copyInResponse" : type == 'H' ? "copyOutResponse" : "copyBothResponse";
                    byte copyFormat = body.Byte();
                    if (copyFormat > 1) throw PgliteProtocol.Invalid("Invalid COPY format.");
                    int copyCount = body.UInt16(); var formats = new List<short>(copyCount);
                    for (int i = 0; i < copyCount; i++)
                    {
                        short format = body.Int16();
                        if (format is not (0 or 1)) throw PgliteProtocol.Invalid("Invalid COPY column format.");
                        formats.Add(format);
                    }
                    var copy = new PgliteCopyResponse(type, copyFormat, formats.AsReadOnly()); copyResponses.Add(copy); value = copy;
                    break;
                case 'd': name = "copyData"; byte[] chunk = body.Bytes(body.Remaining).ToArray(); copyData.Add(chunk); value = chunk; break;
                case 'c': name = "copyDone"; break;
                case 'K': name = "backendKeyData"; value = (body.Int32(), body.Int32()); break;
                case 'V':
                    name = "functionCallResponse";
                    int resultLength = body.Int32();
                    if (resultLength < -1) throw PgliteProtocol.Invalid("Invalid function result length.");
                    value = resultLength == -1 ? null : body.Bytes(resultLength).ToArray();
                    break;
                case 'R': name = "authentication"; value = body.Bytes(body.Remaining).ToArray(); break;
                case 'v':
                    name = "negotiateProtocolVersion"; value = body.Bytes(body.Remaining).ToArray(); break;
                case 'Z':
                    name = "readyForQuery";
                    if (requireReady && columns != null && !descriptionOnly) throw PgliteProtocol.Invalid("Unfinished result before ReadyForQuery.");
                    status = (PgliteTransactionStatus)body.Byte();
                    if (status is not (PgliteTransactionStatus.Idle or PgliteTransactionStatus.InTransaction or PgliteTransactionStatus.FailedTransaction))
                        throw PgliteProtocol.Invalid("Invalid transaction status.");
                    ClearRows(); statementDescription = false; ready = true; value = status;
                    break;
                default: throw PgliteProtocol.Invalid($"Unsupported backend message {type}.");
            }
            if (!body.End) throw PgliteProtocol.Invalid("Trailing message payload.");
            messages.Add(new(type, name, frame, value));
            offset += length + 1;
        }
        if (offset < bytes.Length) pending = bytes.AsSpan(offset).ToArray();
        if (requireReady && (pending.Length != 0 || !ready)) throw PgliteProtocol.Invalid("Missing ReadyForQuery or truncated message.");
        return new(sets, errors, notices, parameters, status, notifications, messages, parameterTypes,
            describedColumns, copyResponses, copyData, ready);
    }

    private void ClearRows()
    {
        columns = null; rows = new(); typedRows = new(); rawRows = new(); descriptionOnly = false;
    }

    private static PgliteError Error(ref PgliteProtocol.Reader body)
    {
        var fields = new Dictionary<char, string>();
        byte field;
        while ((field = body.Byte()) != 0)
        {
            if (field is < 32 or > 126 || !fields.TryAdd((char)field, body.String()))
                throw PgliteProtocol.Invalid("Invalid or duplicate error field.");
        }
        if (!fields.TryGetValue('C', out string? code) || code.Length != 5 ||
            code.Any(c => c is not (>= '0' and <= '9') and not (>= 'A' and <= 'Z')) ||
            !fields.ContainsKey('M') || !fields.ContainsKey('S'))
            throw PgliteProtocol.Invalid("Incomplete PostgreSQL error response.");
        return new(fields);
    }
}

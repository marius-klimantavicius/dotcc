using System.Buffers.Binary;
using System.Text;
using Managed.PGlite;

int checks = 0;
void Check(bool condition, string label)
{
    if (!condition) throw new InvalidOperationException(label);
    checks++;
}
void Reject<T>(Action action, string label) where T : Exception
{
    try { action(); }
    catch (T) { checks++; return; }
    throw new InvalidOperationException("Accepted " + label);
}
byte[] Frame(char type, params byte[] body)
{
    var bytes = new byte[body.Length + 5]; bytes[0] = (byte)type;
    BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(1), body.Length + 4);
    body.CopyTo(bytes, 5); return bytes;
}
byte[] I16(int value) { byte[] b = new byte[2]; BinaryPrimitives.WriteUInt16BigEndian(b, checked((ushort)value)); return b; }
byte[] I32(int value) { byte[] b = new byte[4]; BinaryPrimitives.WriteInt32BigEndian(b, value); return b; }
byte[] Text(string value) => Encoding.UTF8.GetBytes(value + "\0");
byte[] Column(string name, uint oid, short format = 0) => [.. Text(name), .. I32(0), .. I16(0), .. I32((int)oid), 0, 4, .. I32(-1), .. I16(format)];
byte[] Description(params byte[][] columns) => Frame('T', [.. I16(columns.Length), .. columns.SelectMany(c => c)]);
byte[] Row(params byte[]?[] values) => Frame('D', [.. I16(values.Length), .. values.SelectMany(v => v == null ? I32(-1) : [.. I32(v.Length), .. v])]);
byte[] Ready(char state = 'I') => Frame('Z', [(byte)state]);

// Expected vectors follow the pinned pg-protocol serializer tests, rather than
// round-tripping the encoder through our decoder.
Check(PgliteProtocol.EncodeParse("SELECT $1", [23]).SequenceEqual(
    Frame('P', [0, .. Text("SELECT $1"), 0, 1, 0, 0, 0, 23])), "Parse statement and parameter OID vector");
Check(PgliteProtocol.EncodeParse("select 1", [], "named").SequenceEqual(
    Frame('P', [.. Text("named"), .. Text("select 1"), 0, 0])), "Named Parse vector");
Check(PgliteProtocol.EncodeBind([null, "", "Ž"]).SequenceEqual(
    Frame('B', [0, 0, 0, 0, 0, 3, 255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 2, 197, 189, 0, 0])), "Bind NULL, empty and UTF-8 byte lengths");
Check(PgliteProtocol.EncodeBindRaw([new byte[] { 0, 255 }, null], [1], [1]).SequenceEqual(
    Frame('B', [0, 0, 0, 1, 0, 1, 0, 2, 0, 0, 0, 2, 0, 255, 255, 255, 255, 255, 0, 1, 0, 1])), "Binary Bind wire vector");
Check(PgliteProtocol.EncodeDescribe().SequenceEqual(Frame('D', [(byte)'S', 0])), "Describe statement vector");
Check(PgliteProtocol.EncodeDescribe('P', "p").SequenceEqual(Frame('D', [(byte)'P', (byte)'p', 0])), "Describe portal vector");
Check(PgliteProtocol.EncodeExecute("p", 2).SequenceEqual(Frame('E', [(byte)'p', 0, 0, 0, 0, 2])), "Execute row limit vector");
Check(PgliteProtocol.EncodeSync().SequenceEqual(Frame('S')), "Sync vector");
Check(PgliteProtocol.EncodeFlush().SequenceEqual(Frame('H')), "Flush vector");
Check(PgliteProtocol.EncodeClose('P', "p").SequenceEqual(Frame('C', [(byte)'P', (byte)'p', 0])), "Close portal vector");
Check(PgliteProtocol.EncodeCopyData([0, 255]).SequenceEqual(Frame('d', [0, 255])), "COPY data preserves binary bytes");
Check(PgliteProtocol.EncodeCopyDone().SequenceEqual(Frame('c')), "COPY done vector");
Check(PgliteProtocol.EncodeCopyFail("cancelled").SequenceEqual(Frame('f', Text("cancelled"))), "COPY fail vector");
Reject<ArgumentException>(() => PgliteProtocol.EncodeParse("a\0b"), "SQL NUL");
Reject<ArgumentException>(() => PgliteProtocol.EncodeParse("a", statement: "a\0b"), "statement-name NUL");
Reject<EncoderFallbackException>(() => PgliteProtocol.EncodeBind(["\ud800"]), "invalid UTF-16 parameter");
Reject<ArgumentOutOfRangeException>(() => PgliteProtocol.EncodeExecute(maximumRows: -1), "negative Execute limit");
Reject<ArgumentOutOfRangeException>(() => PgliteProtocol.EncodeDescribe('x'), "invalid Describe target");
Reject<ArgumentOutOfRangeException>(() => PgliteProtocol.EncodeParse("a", new uint[65536]), "OID count overflow");
Reject<ArgumentException>(() => PgliteProtocol.EncodeBindRaw([null], [0, 1]), "mismatched format count");
Reject<ArgumentOutOfRangeException>(() => PgliteProtocol.EncodeBindRaw([], resultFormats: [2]), "invalid binary format code");
Check(BinaryPrimitives.ReadUInt16BigEndian(PgliteProtocol.EncodeParse("a", new uint[32768]).AsSpan(8)) == 32768,
    "Protocol counts use unsigned 16-bit range");

var decoder = new PgliteProtocolDecoder();
Check(decoder.Decode(Frame('1')).Messages.Single().Name == "parseComplete", "ParseComplete without Ready");
var described = decoder.Decode(PgliteProtocol.Concat(Frame('t', [0, 2, 0, 0, 0, 23, 0, 0, 0, 16]),
    Description(Column("value", 23), Column("active", 16))));
Check(described.ParameterTypes.SequenceEqual(new uint[] { 23, 16 }) && described.DescribedColumns[1].TypeOid == 16,
    "Server parameter and result OIDs");
Check(!described.HasReadyForQuery && described.ResultSets.Count == 0, "Description stays partial without execution");
Check(decoder.Decode(Frame('2')).Messages.Single().Type == '2', "BindComplete without Ready");
var portal = decoder.Decode(Description(Column("value", 23), Column("active", 16)));
Check(portal.DescribedColumns[0].Name == "value", "Portal description");
object? Decode(string? text, uint oid) => oid == 23 && text != null ? int.Parse(text) : oid == 16 ? text == "t" : text;
var executed = decoder.Decode(PgliteProtocol.Concat(Row("42"u8.ToArray(), "t"u8.ToArray()), Frame('C', Text("SELECT 1"))), Decode);
Check(executed.ResultSets.Single().Rows[0][0] == "42", "Text rows remain compatible");
Check(executed.ResultSets[0].TypedRows[0][0] is int value && value == 42 && executed.ResultSets[0].TypedRows[0][1] is true,
    "Described OIDs drive typed decoding across calls");
Check((int)executed.ResultSets[0].TypedObjectRows[0]["value"]! == 42, "Named typed row mapping");
Check(!executed.HasReadyForQuery && decoder.Decode(Ready('T')).TransactionStatus == PgliteTransactionStatus.InTransaction,
    "Only Sync reports new transaction status");
var saved = executed.Messages[0].Data.ToArray();
decoder.Decode(Frame('1'));
Check(executed.Messages[0].Data.Span.SequenceEqual(saved), "Stream message bytes survive subsequent decode");

byte[] complete = PgliteProtocol.Concat(Description(Column("value", 25), Column("empty", 25), Column("missing", 25)),
    Row(Encoding.UTF8.GetBytes("Žąsis"), [], null), Frame('C', Text("SELECT 1")), Ready());
var simple = PgliteProtocol.DecodeResponse(complete);
Check(simple.ResultSets[0].Rows[0].SequenceEqual(new string?[] { "Žąsis", "", null }), "Unicode, empty, NULL field distinction");
Check(simple.ResultSets[0].RawRows[0][0]!.SequenceEqual(Encoding.UTF8.GetBytes("Žąsis")), "Owned raw field copies");
for (int length = 0; length < complete.Length; length++)
{
    int count = length;
    Reject<InvalidDataException>(() => PgliteProtocol.DecodeResponse(complete.AsSpan(0, count)), "truncated complete response");
}
var incremental = new PgliteProtocolDecoder(); var incrementalMessages = new List<PgliteBackendMessage>();
var incrementalSets = new List<PgliteResultSet>();
foreach (byte nextByte in complete)
{
    var part = incremental.Decode([nextByte]); incrementalMessages.AddRange(part.Messages); incrementalSets.AddRange(part.ResultSets);
}
Check(incrementalMessages.Select(m => m.Type).SequenceEqual(new[] { 'T', 'D', 'C', 'Z' }) && incrementalSets.Single().Rows[0][0] == "Žąsis",
    "Backend frame and UTF-8 fragmentation across every byte");
var copied = PgliteProtocol.DecodeResponse(PgliteProtocol.Concat(Frame('H', [1, 0, 2, 0, 1, 0, 0]),
    Frame('d', [255, 0, 128]), Frame('c'), Frame('C', Text("COPY 2")), Ready()));
Check(copied.CopyResponses.Single().Direction == 'H' && copied.CopyResponses[0].ColumnFormats.SequenceEqual(new short[] { 1, 0 }),
    "COPY response format metadata");
Check(copied.CopyData.Single().SequenceEqual(new byte[] { 255, 0, 128 }), "Binary COPY data avoids text decoding");
Check(new PgliteProtocolDecoder().Decode(Frame('G', [0, 0, 1, 0, 0])).CopyResponses.Single().Direction == 'G', "COPY FROM metadata");
Check(new PgliteProtocolDecoder().Decode(Frame('W', [0, 0, 0])).CopyResponses.Single().Direction == 'W', "COPY BOTH metadata");
var binary = PgliteProtocol.DecodeResponse(PgliteProtocol.Concat(Description(Column("binary", 23, 1)), Row([0, 0, 0, 42]),
    Frame('C', Text("SELECT 1")), Ready()));
Check(binary.ResultSets[0].TypedRows[0][0] is byte[] b && b.SequenceEqual(new byte[] { 0, 0, 0, 42 }) && binary.ResultSets[0].Rows[0][0] == null,
    "Raw binary results retain binary shape");
var events = PgliteProtocol.DecodeResponse(PgliteProtocol.Concat(
    Frame('N', Encoding.UTF8.GetBytes("SNOTICE\0C00000\0Mnotice\0\0")),
    Frame('A', [.. I32(17), .. Text("updates"), .. Text("žinutė")]),
    Frame('S', [.. Text("application_name"), .. Text("test")]), Ready()));
Check(events.Notices.Single().Message == "notice", "Notice fields");
Check(events.Notifications.Single() == new PgliteNotification(17, "updates", "žinutė"), "Notification PID/channel/payload");
Check(events.Parameters["application_name"] == "test", "ParameterStatus");
var errorBytes = Frame('E', Encoding.UTF8.GetBytes("SERROR\0C22P02\0Minvalid integer\0\0"));
var errorDecoder = new PgliteProtocolDecoder(); var error = errorDecoder.Decode(errorBytes);
Check(!error.IsSuccess && !error.HasReadyForQuery && error.Errors[0].SqlState == "22P02", "Extended SQL error remains partial");
Check(errorDecoder.Decode(Ready('E')).TransactionStatus == PgliteTransactionStatus.FailedTransaction, "Sync failed transaction state");
Reject<PgliteSqlException>(() => error.EnsureSuccess(), "SQL exception preserves decoded result");
Reject<InvalidDataException>(() => PgliteProtocol.DecodeResponse(PgliteProtocol.Concat(complete, Ready())), "extra Ready on strict response");
Check(new PgliteProtocolDecoder().Decode(PgliteProtocol.Concat(complete, complete)).ResultSets.Count == 2, "Multiple bundled Simple Query responses");
Reject<InvalidDataException>(() => PgliteProtocol.DecodeResponse(PgliteProtocol.Concat(Frame('d', [0]), Frame('Z', [(byte)'x']))), "bad Ready status");
Reject<InvalidDataException>(() => new PgliteProtocolDecoder().Decode(Frame('H', [2, 0, 0])), "bad COPY format");
Reject<InvalidDataException>(() => PgliteProtocol.DecodeResponse(PgliteProtocol.Concat(Description(Column("value", 25)),
    Row([255]), Frame('C', Text("SELECT 1")), Ready())), "bad UTF-8 text result");

unsafe
{
    using var transport = new PgliteTransport(8);
    byte[] input = [1, 2, 3]; transport.Begin(input);
    byte* bytes = stackalloc byte[8];
    Check(transport.Read(bytes, 2) == 2 && !transport.InputConsumed && transport.InputOffset == 2, "Partial frontend consumption");
    Check(transport.Read(bytes, 8) == 1 && transport.InputConsumed, "Complete frontend consumption");
    bytes[0] = 42;
    Check(transport.Write(bytes, 1) == 1, "Owned streamed transport write");
    var ownedChunk = transport.Chunks[0]; bytes[0] = 99;
    Check(ownedChunk[0] == 42, "Write callback copies borrowed C memory");
    transport.Begin([]);
    Check(ownedChunk[0] == 42 && transport.Chunks.Count == 0, "Previously streamed chunks survive next request");
}
Console.WriteLine($"PASS {checks} ApiProtocol contract assertions (wire/transport only; actual translated engine acceptance belongs to supervisor).");

using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using Managed.PGlite;
using Managed.PGlite.ProductTests;

if (args.Length != 1) throw new ArgumentException("Expected the readonly native-control wire directory containing write/reopen/reopen-again.");
using var cases = SqlCases.Load(Path.Combine(AppContext.BaseDirectory, "cases.json"));
int assertions = 0, transcripts = 0;
void Check(bool condition, string label)
{ if (!condition) throw new InvalidOperationException(label); assertions++; }
void Reject(Action action, string label)
{
    try { action(); }
    catch (InvalidDataException) { assertions++; return; }
    throw new InvalidOperationException("Accepted " + label);
}
byte[] Frame(char type, byte[] payload)
{
    byte[] result = new byte[payload.Length + 5]; result[0] = (byte)type;
    BinaryPrimitives.WriteInt32BigEndian(result.AsSpan(1), payload.Length + 4);
    payload.CopyTo(result, 5); return result;
}
foreach (string group in SqlCases.Groups)
{
    int index = 0;
    foreach (var test in cases.Cases(group).EnumerateArray())
    {
        byte[] wire = File.ReadAllBytes(Path.Combine(args[0], group, $"{index++:000}.wire"));
        var result = PgliteProtocol.DecodeResponse(wire);
        SqlCases.Assert(test, result); transcripts++; assertions++;
        for (int length = 0; length < wire.Length; length++)
        {
            int truncated = length;
            Reject(() => PgliteProtocol.DecodeResponse(wire.AsSpan(0, truncated)), "truncated native frame");
        }
        byte[] trailing = [.. wire, 0];
        Reject(() => PgliteProtocol.DecodeResponse(trailing), "data after ready");
        byte[] badLength = (byte[])wire.Clone(); badLength.AsSpan(1, 4).Clear();
        Reject(() => PgliteProtocol.DecodeResponse(badLength), "invalid native frame length");
        byte[] wrongReady = (byte[])wire.Clone(); wrongReady[^1] = (byte)'!';
        Reject(() => PgliteProtocol.DecodeResponse(wrongReady), "invalid ready state");
    }
}
byte[] encoded = PgliteProtocol.EncodeQuery("SELECT 'Žąsis'");
Check(encoded[0] == 'Q' && encoded[^1] == 0 &&
    BinaryPrimitives.ReadInt32BigEndian(encoded.AsSpan(1)) == encoded.Length - 1 &&
    Encoding.UTF8.GetString(encoded.AsSpan(5, encoded.Length - 6)) == "SELECT 'Žąsis'", "Exact UTF-8 query encoding");
Check(PgliteProtocol.EncodeQuery("").SequenceEqual(new byte[] { (byte)'Q', 0, 0, 0, 5, 0 }), "Empty query encoding");
Check(PgliteProtocol.EncodeTerminate().SequenceEqual(new byte[] { (byte)'X', 0, 0, 0, 4 }), "Terminate encoding");
try { PgliteProtocol.EncodeQuery("a\0b"); throw new InvalidOperationException("Accepted SQL NUL."); }
catch (ArgumentException) { assertions++; }
try { PgliteProtocol.EncodeQuery("\ud800"); throw new InvalidOperationException("Accepted invalid UTF-16."); }
catch (EncoderFallbackException) { assertions++; }
byte[] ready = Frame('Z', [(byte)'I']);
Reject(() => PgliteProtocol.DecodeResponse([.. Frame('D', [0, 1, 0, 0, 0, 1, 49]), .. ready]), "row without description");
Reject(() => PgliteProtocol.DecodeResponse([.. Frame('C', [0xff, 0]), .. ready]), "invalid UTF-8");
Reject(() => PgliteProtocol.DecodeResponse([.. Frame('C', [65]), .. ready]), "unterminated command");
Reject(() => PgliteProtocol.DecodeResponse([.. Frame('C', [65, 0, 0]), .. ready]), "command trailing payload");
Reject(() => PgliteProtocol.DecodeResponse([.. Frame('E', Encoding.UTF8.GetBytes("Mmissing SQLSTATE\0\0")), .. ready]), "incomplete error");
Reject(() => PgliteProtocol.DecodeResponse([.. Frame('E', Encoding.UTF8.GetBytes("SERROR\0C12345\0C12345\0Merror\0\0")), .. ready]), "duplicate SQLSTATE");
Reject(() => PgliteProtocol.DecodeResponse([.. Frame('T', [0xff, 0xff]), .. ready]), "negative column count");
Reject(() => PgliteProtocol.DecodeResponse([.. Frame('?', []), .. ready]), "unknown frame");
Check(PgliteProtocol.DecodeResponse([.. Frame('I', []), .. ready]).ResultSets.Single().Command == "", "EmptyQueryResponse");
var multiple = PgliteProtocol.DecodeResponse([.. Frame('C', "BEGIN\0"u8.ToArray()), .. Frame('C', "COMMIT\0"u8.ToArray()), .. ready]);
Check(multiple.ResultSets.Select(set => set.Command).SequenceEqual(new[] { "BEGIN", "COMMIT" }), "Multiple command completions");
byte[] valuesWire = File.ReadAllBytes(Path.Combine(args[0], "write/002.wire"));
using var incorrect = JsonDocument.Parse("{\"name\":\"wrong-row\",\"rows\":[[\"999\"]],\"commands\":[\"SELECT 2\"],\"ready\":\"I\"}");
Reject(() => SqlCases.Assert(incorrect.RootElement, PgliteProtocol.DecodeResponse(valuesWire)), "deliberately wrong expected row");
unsafe
{
    using var transport = new PgliteTransport(1024 * 1024);
    byte[] request = PgliteProtocol.EncodeQuery("SELECT 'Žąsis'");
    transport.Begin(request);
    // Only the transport owns the request after the local reference is dropped.
    int requestLength = request.Length; request = null!;
    GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
    byte* buffer = stackalloc byte[1024];
    Check(transport.Read(buffer, 3) == 3 && buffer[0] == 'Q' && !transport.InputConsumed, "Short callback read");
    Check(transport.Read(buffer + 3, (1UL << 32) + 8) == requestLength - 3 && transport.InputConsumed,
        "Wide callback length clamps to owned available bytes");
    Check(transport.Read(buffer, 2) == 0, "Callback input EOF");
    Check(transport.Read(null, 0) == 0 && transport.Write(null, 0) == 0, "Zero-length null callback buffers");
    byte[] transcript = valuesWire;
    fixed (byte* p = transcript)
    {
        Check(transport.Write(p, 7) == 7 && transport.Write(p + 7, (ulong)transcript.Length - 7) == transcript.Length - 7,
            "Fragmented callback response copies");
    }
    Check(transport.Response.SequenceEqual(transcript), "Exact copied native callback bytes");
    var copied = PgliteProtocol.DecodeResponse(transport.Response);
    transport.Begin(PgliteProtocol.EncodeQuery("SELECT 2"));
    Check(copied.ResultSets.Single().Rows[0][1] == "Vilnius Žąsis", "Decoded values survive transport reuse");
    Check(transport.Write(buffer, 1UL << 32) == -1 && transport.Error is IOException, "Wide callback write rejects without narrowing");
    Check(transport.Write(buffer, 1) == -1 && transport.Read(buffer, 1) == -1, "Callback failure is sticky");
    transport.Begin([]);
    Check(transport.Read(null, 1) == -1 && transport.Error is IOException, "Null callback read error is contained");
    transport.Begin([]);
    Check(transport.Write(null, 1) == -1 && transport.Error is IOException, "Null callback write error is contained");
    using var bounded = new PgliteTransport(3);
    Check(bounded.Write(buffer, 2) == 2 && bounded.Write(buffer, 2) == -1 && bounded.Response.Length == 2,
        "Response limit preserves already-copied bytes");
}
Console.WriteLine($"PASS {assertions} protocol assertions against {transcripts} retained actual native PostgreSQL wire transcripts; translated SQL execution is unrun by this codec test.");

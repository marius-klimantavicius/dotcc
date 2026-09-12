using Managed.Transport;
using Managed.Transport.Hosting;

internal static unsafe class Program
{
    private static void Main(string[] args)
    {
        Check(args.Length == 1);
        using var host = new MsQuicHost(1);
        host.Install(); // Real callbacks; no transport library or worker startup.
        int cases = 0;
        foreach (string line in File.ReadLines(args[0]))
        {
            string[] parts = line.Split('\t'); Check(parts.Length == 6);
            string id = parts[0], kind = parts[1];
            byte server = byte.Parse(parts[2]); int repeats = int.Parse(parts[3]);
            bool expected = parts[4] == "1"; byte[] bytes = Convert.FromHexString(parts[5]);
            bool ok = false; ushort offset = 1; ulong a = 0, b = 0, c = 0;
            string data = "-";
            fixed (byte* input = bytes)
            {
                if (kind.StartsWith("tp", StringComparison.Ordinal))
                {
                    QUIC_TRANSPORT_PARAMETERS tp = default;
                    try
                    {
                        for (int n = 0; n < repeats; ++n)
                        {
                            ok = MsQuic.QuicCryptoTlsDecodeTransportParameters(null, server, input, (ushort)bytes.Length, &tp) != 0;
                            Check(ok == expected);
                        }
                        if (kind == "tp-replace")
                        {
                            Check(host.OutstandingPlatformAllocations == 1);
                            byte invalid = 255;
                            ok = MsQuic.QuicCryptoTlsDecodeTransportParameters(null, server, &invalid, 1, &tp) != 0;
                            Check(!ok && host.OutstandingPlatformAllocations == 0);
                        }
                        a = tp.Flags; b = tp.CibirLength; c = tp.CibirOffset;
                        data = Hex(tp.VersionInfo, checked((int)tp.VersionInfoLength));
                        Console.Write($"{id}|{(ok ? 1 : 0)}|NA|{a}|{b}|{c}|{data}|{host.OutstandingPlatformAllocations}|");
                    }
                    finally { MsQuic.QuicCryptoTlsCleanupTransportParameters(&tp); }
                    Check(host.OutstandingPlatformAllocations == 0);
                    Console.WriteLine("0"); ++cases; continue;
                }
                switch (kind)
                {
                    case "reset":
                        QUIC_RESET_STREAM_EX reset = default;
                        ok = MsQuic.QuicResetStreamFrameDecode((ushort)bytes.Length, input, &offset, &reset) != 0;
                        if (ok) { a = reset.StreamID; b = reset.ErrorCode; c = reset.FinalSize; }
                        break;
                    case "stop":
                        QUIC_STOP_SENDING_EX stop = default;
                        ok = MsQuic.QuicStopSendingFrameDecode((ushort)bytes.Length, input, &offset, &stop) != 0;
                        if (ok) { a = stop.StreamID; b = stop.ErrorCode; }
                        break;
                    case "crypto":
                        QUIC_CRYPTO_EX crypto = default;
                        ok = MsQuic.QuicCryptoFrameDecode((ushort)bytes.Length, input, &offset, &crypto) != 0;
                        if (ok) { a = crypto.Offset; b = crypto.Length; c = checked((ulong)(crypto.Data - input)); data = Hex(crypto.Data, checked((int)crypto.Length)); }
                        break;
                    case "maxdata":
                        QUIC_MAX_DATA_EX max = default;
                        ok = MsQuic.QuicMaxDataFrameDecode((ushort)bytes.Length, input, &offset, &max) != 0;
                        if (ok) a = max.MaximumData;
                        break;
                    default: throw new InvalidOperationException("Unknown decoder kind");
                }
            }
            Check(ok == expected && offset <= bytes.Length && host.OutstandingPlatformAllocations == 0);
            Console.WriteLine($"{id}|{(ok ? 1 : 0)}|{offset}|{a}|{b}|{c}|{data}|0|0"); ++cases;
        }
        Check(host.OutstandingPlatformAllocations == 0 && host.OutstandingResources == 0);
        host.Dispose();
        Console.WriteLine($"PASS malformed corpus cases={cases} allocations=0");
    }
    private static string Hex(byte* data, int length)
        => length == 0 ? "-" : Convert.ToHexStringLower(new ReadOnlySpan<byte>(data, length));
    private static void Check(bool value)
    { if (!value) throw new InvalidOperationException("Malformed corpus invariant failed"); }
}

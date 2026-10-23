using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Win32.SafeHandles;
using Managed.Security.S2n;
using ManagedConsumer;

try
{
    if (args.Length == 9 && args[0] == "--endpoint")
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var socket = new Socket(new SafeSocketHandle((nint)int.Parse(args[2]), ownsHandle: true));
        var result = await Exchange.EndpointAsync(args[1] == "client" ? S2nRole.Client : S2nRole.Server, socket,
            Exchange.Protocol(args[3]), Convert.ToUInt16(args[4].Replace("0x", ""), 16), args[5], args[6], args[7], args[8], timeout.Token);
        Console.WriteLine(JsonSerializer.Serialize(result, ConsumerJson.Default.ExchangeObservation));
        return 0;
    }
    if (args.Length != 4 || args[0] != "--fixtures" || args[2] != "--source")
        throw new ArgumentException("Usage: ManagedConsumer --fixtures <native/credentials> --source <s2n-source> | --endpoint <client|server> <fd> <S2N_version> <0xIANA> <cipher-name> <credentials> <source> <validate|untrusted|bypass-untrusted>");
    foreach (var test in new[] {
        (S2nProtocol.Ssl3, (ushort)0x0004, "RC4-MD5"),
        (S2nProtocol.Ssl3, (ushort)0x000a, "DES-CBC3-SHA"),
        (S2nProtocol.Tls13, (ushort)0x1301, "TLS_AES_128_GCM_SHA256") })
    {
        foreach (var result in await Exchange.LoopbackAsync(test.Item1, test.Item2, test.Item3, args[1], args[3]))
            Console.WriteLine(JsonSerializer.Serialize(result, ConsumerJson.Default.ExchangeObservation));
    }
    var runtime = S2nRuntime.Inspect();
    if (runtime.References != 0 || runtime.Initializations != runtime.FinalCleanups || runtime.Failure != null)
        throw new InvalidOperationException("Global owners or final cleanup did not drain: " + runtime);
    Console.WriteLine("PASS: SSLv3/RC4, SSLv3/3DES, TLS1.3 loopback; binary payload, negotiated values, close_notify and cleanup_final asserted.");
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine(error);
    return 1;
}

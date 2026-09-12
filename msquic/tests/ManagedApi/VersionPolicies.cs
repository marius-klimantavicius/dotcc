using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Managed.Transport.Api;

// Move into the ManagedApi test glob only at a serialized source boundary.
internal static class VersionPolicyControls
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(20);
    internal static async Task RunAsync()
    {
        using var certificates = new Credentials();
        await using var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 1 });
        await using var registration = await runtime.OpenRegistrationAsync("scoped-version-policies");
        using var serverCredentials = QuicCredentials.Server(certificates.EcLeaf);
        using var clientCredentials = QuicCredentials.Client([certificates.Root], X509RevocationMode.NoCheck);
        await using var serverConfig = await registration.CreateConfigurationAsync(["version-policies"u8.ToArray()], serverCredentials);
        await using var clientConfig = await registration.CreateConfigurationAsync(["version-policies"u8.ToArray()], clientCredentials);
        CheckOwner(serverConfig.GetVersionPolicy, serverConfig.SetVersionPolicy);
        CheckOwner(clientConfig.GetVersionPolicy, clientConfig.SetVersionPolicy);
        await using var listener = await registration.ListenAsync(serverConfig, new(IPAddress.Loopback, 0));
        using var timeout = new CancellationTokenSource(Limit);
        Task<QuicConnection> accepting = listener.AcceptConnectionAsync(timeout.Token).AsTask();
        await using var client = await registration.ConnectAsync(clientConfig, "localhost", listener.LocalEndPoint, timeout.Token);
        await using var server = await accepting.WaitAsync(Limit);
        // The pinned setters have no blanket post-start rejection: all requests
        // go through the original converter/apply/state checks on real handles.
        CheckOwner(client.GetVersionPolicy, client.SetVersionPolicy);
        CheckOwner(server.GetVersionPolicy, server.SetVersionPolicy);
        CheckOwner(serverConfig.GetVersionPolicy, serverConfig.SetVersionPolicy);
        Check(client.GetProtocolVersion() == 1 && server.GetProtocolVersion() == 1,
            "updating effective lists changed the negotiated version");
        await registration.DisposeAsync().AsTask().WaitAsync(Limit);
        CheckClosed(serverConfig.GetVersionPolicy, serverConfig.SetVersionPolicy);
        CheckClosed(clientConfig.GetVersionPolicy, clientConfig.SetVersionPolicy);
        CheckClosed(client.GetVersionPolicy, client.SetVersionPolicy);
        CheckClosed(server.GetVersionPolicy, server.SetVersionPolicy);
        await runtime.DisposeAsync().AsTask().WaitAsync(Limit);
        Check(runtime.Host.OutstandingResources == 0 && runtime.Host.OutstandingPlatformAllocations == 0,
            "version-policy controls leaked host allocation or handle ownership");
        Console.WriteLine("PASS facade scoped version policies: actual config/connection lists, updates, copies, rejection and lifetime");
    }

    private static void CheckOwner(Func<QuicParameterPriority, QuicVersionPolicy> get,
        Action<QuicVersionPolicy, QuicParameterPriority> set)
    {
        CheckV1(get(QuicParameterPriority.Normal));
        uint[] a = [1], o = [1], f = [1];
        set(new(a, o, f), QuicParameterPriority.High);
        a[0] = 2; o[0] = 2; f[0] = 2;
        CheckV1(get(QuicParameterPriority.High));
        var copied = get(QuicParameterPriority.Normal);
        MutateSnapshot(copied.AcceptableVersions);
        MutateSnapshot(copied.OfferedVersions);
        MutateSnapshot(copied.FullyDeployedVersions);
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        CheckV1(get(QuicParameterPriority.Normal));
        foreach (var invalid in new[]
        {
            new QuicVersionPolicy(new uint[] { 1 }, new uint[] { 2 }, new uint[] { 1 }),
            new QuicVersionPolicy(new uint[] { 1 }, new uint[] { 1 }, new uint[] { 2 }),
            new QuicVersionPolicy(Array.Empty<uint>(), new uint[] { 1 }, new uint[] { 1 }),
            new QuicVersionPolicy(new uint[] { 1, 1 }, new uint[] { 1 }, new uint[] { 1 })
        })
        {
            Throws<NotSupportedException>(() => set(invalid, QuicParameterPriority.High));
            CheckV1(get(QuicParameterPriority.Normal));
        }
        Throws<ArgumentOutOfRangeException>(() => set(V1(), (QuicParameterPriority)2));
        Throws<ArgumentOutOfRangeException>(() => get((QuicParameterPriority)2));
        Throws<ArgumentNullException>(() => set(null!, QuicParameterPriority.Normal));
        CheckV1(get(QuicParameterPriority.Normal));
    }
    private static void CheckClosed(Func<QuicParameterPriority, QuicVersionPolicy> get,
        Action<QuicVersionPolicy, QuicParameterPriority> set)
    {
        Throws<ObjectDisposedException>(() => get(QuicParameterPriority.Normal));
        Throws<ObjectDisposedException>(() => set(V1(), QuicParameterPriority.Normal));
    }
    private static void MutateSnapshot(ReadOnlyMemory<uint> values)
    {
        Check(MemoryMarshal.TryGetArray(values, out ArraySegment<uint> segment) && segment.Count == 1,
            "version getter did not return independent array storage");
        segment.Array![segment.Offset] = 2;
    }
    private static QuicVersionPolicy V1() => new(new uint[] { 1 }, new uint[] { 1 }, new uint[] { 1 });
    private static void CheckV1(QuicVersionPolicy policy)
    {
        Check(policy.AcceptableVersions.Span.SequenceEqual(new uint[] { 1 }) &&
              policy.OfferedVersions.Span.SequenceEqual(new uint[] { 1 }) &&
              policy.FullyDeployedVersions.Span.SequenceEqual(new uint[] { 1 }), "effective version policy was not exactly v1");
    }
    private static void Throws<T>(Action operation) where T : Exception
    {
        try { operation(); }
        catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }
    private static void Check(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
}

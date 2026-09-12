using System.Security.Cryptography;
using Managed.Transport.Api;

internal static class RuntimeParameterControls
{
    internal static async Task InvalidCreationAsync()
    {
        try
        {
            await using var runtime = await QuicRuntime.CreateAsync(new()
            {
                Parameters = new() { RetryMemoryLimit = 0, LoadBalancingMode = (QuicLoadBalancingMode)1 }
            });
        }
        catch (NotSupportedException) { return; }
        throw new InvalidOperationException("Unsupported runtime parameter was accepted before host installation.");
    }

    internal static void BeforeRegistration(QuicRuntime runtime)
    {
        var version = runtime.GetLibraryVersion();
        Require(version.Major == 2 && version.Minor == 7 && version.Patch == 0, "Pinned library version");
        // Exercise the actual getter without manufacturing pin metadata. The
        // final regeneration must set VER_GIT_HASH to the immutable source pin;
        // current objects still carry upstream's default "Unknown".
        Require(runtime.GetLibrarySourceRevision() == Managed.Transport.MsQuic.VER_GIT_HASH_STR,
            "Actual compiled library revision metadata");
        Require(runtime.GetTlsProvider() == QuicTlsProvider.Picotls, "Explicit picotls provider identity");
        Require(runtime.GetCompiledProtocolVersions().Contains(1u), "Compiled version host-order decoding");
        var policy = runtime.GetVersionPolicy();
        CheckV1(policy);
        Reject<NotSupportedException>(() => runtime.SetVersionPolicy(new(new uint[] { 1 }, new uint[] { 2 }, new uint[] { 1 })));
        CheckV1(runtime.GetVersionPolicy());
        Reject<NotSupportedException>(() => runtime.SetVersionPolicy(new(Array.Empty<uint>(), new uint[] { 1 }, new uint[] { 1 })));
        runtime.SetVersionPolicy(policy);
        // Returned version storage is a managed snapshot, independent of core
        // pointers and the next collection/moving-GC cycle.
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        CheckV1(policy);

        var original = runtime.GetParameters();
        Require(original.LoadBalancingMode == QuicLoadBalancingMode.Disabled, "Initial load balancing policy");
        Reject<NotSupportedException>(() => runtime.SetParameters(new() { RetryMemoryLimit = 0, LoadBalancingMode = (QuicLoadBalancingMode)2 }));
        Require(runtime.GetParameters() == original, "Invalid combined global update mutated Retry limit");
        runtime.SetRetryMemoryLimit(0);
        Require(runtime.GetRetryMemoryLimit() == 0 && runtime.GetParameters().RetryMemoryLimit == 0, "Zero Retry threshold round trip");
        runtime.SetParameters(new() { RetryMemoryLimit = 1234 });
        Require(runtime.GetRetryMemoryLimit() == 1234 && runtime.GetLoadBalancingMode() == QuicLoadBalancingMode.Disabled,
            "Partial global update preserved load balancing");
        runtime.SetLoadBalancingMode(QuicLoadBalancingMode.Disabled);
        Reject<NotSupportedException>(() => runtime.SetLoadBalancingMode((QuicLoadBalancingMode)3));
        runtime.SetParameters(original);

        var settings = runtime.GetSettings();
        Reject<ArgumentException>(() => runtime.SetSettings(new() { IdleTimeoutMs = 98765, StreamRecvWindowDefault = 3 }));
        Require(runtime.GetSettings() == settings, "Global rejected later field mutated earlier timeout");
        runtime.SetSettings(new() { IdleTimeoutMs = 45678 });
        Require(runtime.GetSettings().IdleTimeoutMs == 45678, "Global settings update");
        runtime.SetSettings(new() { IdleTimeoutMs = settings.IdleTimeoutMs });

        var sizes = runtime.GetStatisticsV2Sizes();
        Require(sizes.Length == 5 && sizes[0] > 0, "Actual statistics revision sizes");
        for (int i = 1; i < sizes.Length; i++) Require(sizes[i] > sizes[i - 1], "Statistics extents increase");
        var counters = runtime.GetPerformanceCounters();
        Require(counters.Count == 35 && counters[QuicPerformanceCounter.ActiveConnections] == 0, "Actual counter extent and idle connections");
        Reject<ArgumentOutOfRangeException>(() => _ = counters[(QuicPerformanceCounter)999]);

        Reject<ArgumentException>(() => runtime.ProvisionStatelessResetKey(new byte[31]));
        try { runtime.ProvisionStatelessResetKey(new byte[32]); }
        catch (QuicTransportException error) when (error.Status == 1) { return; }
        throw new InvalidOperationException("Reset key provisioning ignored upstream lazy-initialization restriction.");
    }

    internal static void AfterRegistration(QuicRuntime runtime)
    {
        byte[] secret = RandomNumberGenerator.GetBytes(32);
        try
        {
            runtime.ProvisionStatelessResetKey(secret);
            runtime.ConfigureStatelessRetry(QuicPacketCipher.Aes128Gcm, 1000, secret.AsSpan(0, 16));
            runtime.ConfigureStatelessRetry(QuicPacketCipher.Aes256Gcm, 2000, secret);
            Reject<ArgumentException>(() => runtime.ConfigureStatelessRetry(QuicPacketCipher.Aes128Gcm, 1000, secret));
            Reject<ArgumentOutOfRangeException>(() => runtime.ConfigureStatelessRetry(QuicPacketCipher.Aes256Gcm, 0, secret));
            Reject<NotSupportedException>(() => runtime.ConfigureStatelessRetry((QuicPacketCipher)2, 1000, secret));
        }
        finally { CryptographicOperations.ZeroMemory(secret); }
        runtime.SetLoadBalancingMode(QuicLoadBalancingMode.Disabled);
        Console.WriteLine("PASS managed runtime parameters: actual library/profile/counter queries; global setting atomicity; v1 policy; Retry/reset provisioning");
    }

    private static void CheckV1(QuicVersionPolicy policy)
    {
        Require(policy.AcceptableVersions.Span.SequenceEqual(new uint[] { 1 }) &&
            policy.OfferedVersions.Span.SequenceEqual(new uint[] { 1 }) &&
            policy.FullyDeployedVersions.Span.SequenceEqual(new uint[] { 1 }), "Effective version policy");
    }

    private static void Reject<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }
    private static void Require(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
}

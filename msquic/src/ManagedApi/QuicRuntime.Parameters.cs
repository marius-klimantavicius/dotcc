using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Managed.Transport.Hosting;

namespace Managed.Transport.Api;

public enum QuicLoadBalancingMode : ushort { Disabled = 0 }
public enum QuicPacketCipher { Aes128Gcm = 0, Aes256Gcm = 1 }
public enum QuicTlsProvider { Picotls = 0x10000 }

public sealed record QuicRuntimeParameters
{
    /// <summary>Fraction of host memory available for handshakes: 0 forces
    /// Retry, and 65535 represents 100 percent. This is the native fixed-point
    /// threshold, despite the historical parameter name containing PERCENT.</summary>
    public ushort? RetryMemoryLimit { get; init; }
    public QuicLoadBalancingMode? LoadBalancingMode { get; init; }
}

public sealed record QuicLibraryVersion(uint Major, uint Minor, uint Patch, uint Build);
public sealed record QuicVersionPolicy(ReadOnlyMemory<uint> AcceptableVersions,
    ReadOnlyMemory<uint> OfferedVersions, ReadOnlyMemory<uint> FullyDeployedVersions);

/// <summary>Indices of the actual translated global counters. A counter may
/// remain zero when no corresponding work occurred; it does not imply support.</summary>
public enum QuicPerformanceCounter
{
    ConnectionsCreated = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_CONN_CREATED,
    FailedHandshakes = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_CONN_HANDSHAKE_FAIL,
    ConnectionsRejectedByApplication = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_CONN_APP_REJECT,
    ConnectionsResumed = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_CONN_RESUMED,
    ActiveConnections = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_CONN_ACTIVE,
    ConnectedConnections = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_CONN_CONNECTED,
    ProtocolErrors = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_CONN_PROTOCOL_ERRORS,
    ConnectionsWithNoAlpn = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_CONN_NO_ALPN,
    ActiveStreams = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_STRM_ACTIVE,
    SuspectedLostPackets = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_PKTS_SUSPECTED_LOST,
    DroppedPackets = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_PKTS_DROPPED,
    DecryptionFailures = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_PKTS_DECRYPTION_FAIL,
    ReceivedDatagrams = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_UDP_RECV,
    SentDatagrams = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_UDP_SEND,
    ReceivedDatagramBytes = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_UDP_RECV_BYTES,
    SentDatagramBytes = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_UDP_SEND_BYTES,
    DatagramReceiveEvents = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_UDP_RECV_EVENTS,
    DatagramSendCalls = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_UDP_SEND_CALLS,
    ApplicationSentBytes = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_APP_SEND_BYTES,
    ApplicationReceivedBytes = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_APP_RECV_BYTES,
    ConnectionQueueDepth = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_CONN_QUEUE_DEPTH,
    ConnectionOperationQueueDepth = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_CONN_OPER_QUEUE_DEPTH,
    ConnectionOperationsQueued = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_CONN_OPER_QUEUED,
    ConnectionOperationsCompleted = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_CONN_OPER_COMPLETED,
    WorkerOperationQueueDepth = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_WORK_OPER_QUEUE_DEPTH,
    WorkerOperationsQueued = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_WORK_OPER_QUEUED,
    WorkerOperationsCompleted = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_WORK_OPER_COMPLETED,
    ValidatedPaths = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_PATH_VALIDATED,
    FailedPaths = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_PATH_FAILURE,
    StatelessResetsSent = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_SEND_STATELESS_RESET,
    StatelessRetriesSent = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_SEND_STATELESS_RETRY,
    ConnectionsRejectedByLoad = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_CONN_LOAD_REJECT,
    ListenerQueueDepth = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_LISTEN_QUEUE_DEPTH,
    EncryptionMicroseconds = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_ENCRYPT_DURATION_US,
    DecryptionMicroseconds = (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_DECRYPT_DURATION_US,
}

public sealed class QuicPerformanceCounters
{
    private readonly long[] values;
    internal QuicPerformanceCounters(long[] values) => this.values = values;
    public int Count => values.Length;
    public long this[QuicPerformanceCounter counter]
    {
        get
        {
            int index = (int)counter;
            if ((uint)index >= values.Length) throw new ArgumentOutOfRangeException(nameof(counter));
            return values[index];
        }
    }
}

public sealed partial class QuicRuntime
{
    private readonly object parameterGate = new();

    private static void ValidateRuntimeParameters(QuicRuntimeParameters? parameters)
    {
        if (parameters?.LoadBalancingMode is { } mode && mode != QuicLoadBalancingMode.Disabled)
            throw new NotSupportedException("Only disabled load balancing is selected for this QUIC profile.");
    }

    private void InitializeRuntimeParameters(QuicRuntimeParameters? parameters)
        => SetParameters((parameters ?? new()) with { LoadBalancingMode = QuicLoadBalancingMode.Disabled });

    private IDisposable EnterParameterOperation()
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(closing, this);
            var operation = new QuicBudgetLease(() =>
            {
                lock (gate) if (--admissions == 0) admissionsDrained?.TrySetResult();
            });
            admissions = checked(admissions + 1);
            return operation;
        }
    }

    public unsafe QuicRuntimeParameters GetParameters()
    {
        using var operation = EnterParameterOperation();
        lock (parameterGate)
        {
            var value = ReadGlobal<QUIC_GLOBAL_SETTINGS>(MsQuic.QUIC_PARAM_GLOBAL_GLOBAL_SETTINGS);
            return new() { RetryMemoryLimit = value.RetryMemoryLimit, LoadBalancingMode = (QuicLoadBalancingMode)value.LoadBalancingMode };
        }
    }

    public unsafe void SetParameters(QuicRuntimeParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ValidateRuntimeParameters(parameters);
        QUIC_GLOBAL_SETTINGS native = default;
        var mask = native.IsSet;
        if (parameters.RetryMemoryLimit is { } limit) { mask.RetryMemoryLimit = 1; native.RetryMemoryLimit = limit; }
        if (parameters.LoadBalancingMode is { } mode) { mask.LoadBalancingMode = 1; native.LoadBalancingMode = (ushort)mode; }
        native.IsSet = mask;
        using var operation = EnterParameterOperation();
        lock (parameterGate)
            QuicError.ThrowIfFailed(Api->SetParam(null, MsQuic.QUIC_PARAM_GLOBAL_GLOBAL_SETTINGS,
                (uint)sizeof(QUIC_GLOBAL_SETTINGS), &native), "global runtime parameters");
    }

    public unsafe ushort GetRetryMemoryLimit()
    {
        using var operation = EnterParameterOperation();
        lock (parameterGate) return ReadGlobal<ushort>(MsQuic.QUIC_PARAM_GLOBAL_RETRY_MEMORY_PERCENT);
    }

    public unsafe void SetRetryMemoryLimit(ushort threshold)
    {
        using var operation = EnterParameterOperation();
        lock (parameterGate)
            QuicError.ThrowIfFailed(Api->SetParam(null, MsQuic.QUIC_PARAM_GLOBAL_RETRY_MEMORY_PERCENT,
                sizeof(ushort), &threshold), "Retry memory limit");
    }

    public unsafe QuicLoadBalancingMode GetLoadBalancingMode()
    {
        using var operation = EnterParameterOperation();
        lock (parameterGate) return (QuicLoadBalancingMode)ReadGlobal<ushort>(MsQuic.QUIC_PARAM_GLOBAL_LOAD_BALACING_MODE);
    }

    public unsafe void SetLoadBalancingMode(QuicLoadBalancingMode mode)
    {
        if (mode != QuicLoadBalancingMode.Disabled) throw new NotSupportedException("Only disabled QUIC load balancing is selected.");
        ushort value = (ushort)mode;
        using var operation = EnterParameterOperation();
        lock (parameterGate)
            QuicError.ThrowIfFailed(Api->SetParam(null, MsQuic.QUIC_PARAM_GLOBAL_LOAD_BALACING_MODE,
                sizeof(ushort), &value), "load balancing mode");
    }

    public unsafe QuicSettings GetSettings()
    {
        using var operation = EnterParameterOperation();
        lock (parameterGate) return QuicSettings.FromNative(ReadGlobal<QUIC_SETTINGS>(MsQuic.QUIC_PARAM_GLOBAL_SETTINGS));
    }

    public unsafe void SetSettings(QuicSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        QUIC_SETTINGS requested = settings.ToNative();
        using var operation = EnterParameterOperation();
        lock (parameterGate)
        {
            var effective = ReadGlobal<QUIC_SETTINGS>(MsQuic.QUIC_PARAM_GLOBAL_SETTINGS);
            QuicSettings.ValidateUpdate(requested, effective);
            QuicError.ThrowIfFailed(Api->SetParam(null, MsQuic.QUIC_PARAM_GLOBAL_SETTINGS,
                (uint)sizeof(QUIC_SETTINGS), &requested), "global QUIC settings");
        }
    }

    /// <summary>Provision a copied 32-byte reset key after the core's lazy
    /// initialization. Earlier calls preserve its INVALID_STATE error. This
    /// operation has no secret-bearing getter.</summary>
    public unsafe void ProvisionStatelessResetKey(ReadOnlySpan<byte> key)
    {
        if (key.Length != 32) throw new ArgumentException("A stateless reset key must contain 32 bytes.", nameof(key));
        Span<byte> copy = stackalloc byte[32]; key.CopyTo(copy);
        try
        {
            using var operation = EnterParameterOperation();
            lock (parameterGate)
                fixed (byte* data = copy)
                    QuicError.ThrowIfFailed(Api->SetParam(null, MsQuic.QUIC_PARAM_GLOBAL_STATELESS_RESET_KEY, 32, data), "stateless reset key");
        }
        finally { CryptographicOperations.ZeroMemory(copy); }
    }

    /// <summary>Install the Retry secret and rotation interval through the
    /// translated core. AES-128 needs 16 bytes; AES-256 needs 32 bytes. Existing
    /// active Retry keys follow the upstream rotation policy.</summary>
    public unsafe void ConfigureStatelessRetry(QuicPacketCipher cipher, uint rotationMilliseconds, ReadOnlySpan<byte> secret)
    {
        int required = cipher switch { QuicPacketCipher.Aes128Gcm => 16, QuicPacketCipher.Aes256Gcm => 32, _ => throw new NotSupportedException("Unsupported Retry cipher.") };
        if (secret.Length != required) throw new ArgumentException("Retry secret length does not match its cipher.", nameof(secret));
        if (rotationMilliseconds == 0) throw new ArgumentOutOfRangeException(nameof(rotationMilliseconds));
        Span<byte> copy = stackalloc byte[32]; copy.Clear(); secret.CopyTo(copy);
        try
        {
            using var operation = EnterParameterOperation();
            lock (parameterGate)
                fixed (byte* data = copy)
                {
                    QUIC_STATELESS_RETRY_CONFIG config = new()
                    {
                        Algorithm = (QUIC_AEAD_ALGORITHM_TYPE)cipher, RotationMs = rotationMilliseconds,
                        SecretLength = (uint)required, Secret = data
                    };
                    QuicError.ThrowIfFailed(Api->SetParam(null, MsQuic.QUIC_PARAM_GLOBAL_STATELESS_RETRY_CONFIG,
                        (uint)sizeof(QUIC_STATELESS_RETRY_CONFIG), &config), "stateless Retry configuration");
                }
        }
        finally { CryptographicOperations.ZeroMemory(copy); }
    }

    /// <summary>Compiled protocol versions in host numeric order. This list is
    /// an upstream observation; the effective managed policy remains v1 only.</summary>
    public uint[] GetCompiledProtocolVersions()
    {
        using var operation = EnterParameterOperation();
        lock (parameterGate)
        {
            byte[] bytes = ReadGlobalBytes(MsQuic.QUIC_PARAM_GLOBAL_SUPPORTED_VERSIONS);
            if (bytes.Length % 4 != 0) throw new InvalidOperationException("Invalid compiled version list extent.");
            uint[] result = new uint[bytes.Length / 4];
            for (int i = 0; i < result.Length; i++) result[i] = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(i * 4, 4));
            return result;
        }
    }

    public unsafe QuicVersionPolicy GetVersionPolicy()
    {
        using var operation = EnterParameterOperation();
        lock (parameterGate)
        {
            byte[] bytes = new byte[GlobalBufferLength(MsQuic.QUIC_PARAM_GLOBAL_VERSION_SETTINGS)];
            fixed (byte* buffer = bytes)
            {
                uint length = (uint)bytes.Length;
                QuicError.ThrowIfFailed(Api->GetParam(null, MsQuic.QUIC_PARAM_GLOBAL_VERSION_SETTINGS, &length, buffer), "global version policy");
                if (length < sizeof(QUIC_VERSION_SETTINGS) || length > bytes.Length) throw new InvalidOperationException("Invalid version policy extent.");
                var value = (QUIC_VERSION_SETTINGS*)buffer;
                return new(ReadVersionList(value->AcceptableVersions, value->AcceptableVersionsLength, buffer, length),
                    ReadVersionList(value->OfferedVersions, value->OfferedVersionsLength, buffer, length),
                    ReadVersionList(value->FullyDeployedVersions, value->FullyDeployedVersionsLength, buffer, length));
            }
        }
    }

    public unsafe void SetVersionPolicy(QuicVersionPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        // Snapshot before validation: caller-owned memory cannot change between
        // validation and dispatch. Empty/default lists would restore upstream v2.
        uint[] acceptable = policy.AcceptableVersions.ToArray(), offered = policy.OfferedVersions.ToArray(), fully = policy.FullyDeployedVersions.ToArray();
        foreach (uint[] list in new[] { acceptable, offered, fully })
            if (list.Length != 1 || list[0] != 1) throw new NotSupportedException("Each effective version list must contain exactly QUIC v1.");
        using var operation = EnterParameterOperation();
        lock (parameterGate) SetVersionOne(Api, null, MsQuic.QUIC_PARAM_GLOBAL_VERSION_SETTINGS);
    }

    public QuicLibraryVersion GetLibraryVersion()
    {
        using var operation = EnterParameterOperation();
        lock (parameterGate)
        {
            byte[] bytes = ReadGlobalBytes(MsQuic.QUIC_PARAM_GLOBAL_LIBRARY_VERSION);
            if (bytes.Length != 16) throw new InvalidOperationException("Invalid library version extent.");
            ReadOnlySpan<uint> values = MemoryMarshal.Cast<byte, uint>(bytes);
            return new(values[0], values[1], values[2], values[3]);
        }
    }

    public string GetLibrarySourceRevision()
    {
        using var operation = EnterParameterOperation();
        lock (parameterGate)
        {
            byte[] bytes = ReadGlobalBytes(MsQuic.QUIC_PARAM_GLOBAL_LIBRARY_GIT_HASH);
            if (bytes.Length < 2 || bytes[^1] != 0 || bytes.AsSpan(0, bytes.Length - 1).Contains((byte)0))
                throw new InvalidOperationException("Invalid library source revision string.");
            return Encoding.ASCII.GetString(bytes, 0, bytes.Length - 1);
        }
    }

    public unsafe QuicTlsProvider GetTlsProvider()
    {
        using var operation = EnterParameterOperation();
        lock (parameterGate) return (QuicTlsProvider)ReadGlobal<QUIC_TLS_PROVIDER>(MsQuic.QUIC_PARAM_GLOBAL_TLS_PROVIDER);
    }

    public QuicPerformanceCounters GetPerformanceCounters()
    {
        using var operation = EnterParameterOperation();
        lock (parameterGate)
        {
            byte[] bytes = ReadGlobalBytes(MsQuic.QUIC_PARAM_GLOBAL_PERF_COUNTERS);
            if (bytes.Length != (int)QUIC_PERFORMANCE_COUNTERS.QUIC_PERF_COUNTER_MAX * sizeof(long))
                throw new InvalidOperationException("Unexpected global counter extent.");
            return new(MemoryMarshal.Cast<byte, long>(bytes).ToArray());
        }
    }

    public uint[] GetStatisticsV2Sizes()
    {
        using var operation = EnterParameterOperation();
        lock (parameterGate)
        {
            byte[] bytes = ReadGlobalBytes(MsQuic.QUIC_PARAM_GLOBAL_STATISTICS_V2_SIZES);
            if (bytes.Length % 4 != 0) throw new InvalidOperationException("Invalid statistics size list extent.");
            return MemoryMarshal.Cast<byte, uint>(bytes).ToArray();
        }
    }

    private unsafe T ReadGlobal<T>(uint parameter) where T : unmanaged
    {
        T value = default; uint length = (uint)sizeof(T);
        QuicError.ThrowIfFailed(Api->GetParam(null, parameter, &length, &value), "global parameter query");
        if (length != sizeof(T)) throw new InvalidOperationException("Unexpected global parameter extent.");
        return value;
    }

    private unsafe int GlobalBufferLength(uint parameter)
    {
        uint length = 0;
        uint status = Api->GetParam(null, parameter, &length, null);
        if (status != Status.BufferTooSmall) { QuicError.ThrowIfFailed(status, "global parameter size query"); throw new InvalidOperationException("Missing global parameter size."); }
        if (length == 0 || length > 65536) throw new InvalidOperationException("Unbounded global parameter result.");
        return (int)length;
    }

    private unsafe byte[] ReadGlobalBytes(uint parameter)
    {
        byte[] result = new byte[GlobalBufferLength(parameter)];
        fixed (byte* buffer = result)
        {
            uint length = (uint)result.Length;
            QuicError.ThrowIfFailed(Api->GetParam(null, parameter, &length, buffer), "global parameter query");
            if (length > result.Length) throw new InvalidOperationException("Global parameter overran its extent.");
            if (length != result.Length) Array.Resize(ref result, (int)length);
        }
        return result;
    }

    private static unsafe uint[] ReadVersionList(uint* values, uint count, byte* buffer, uint length)
    {
        nuint offset = (nuint)values - (nuint)buffer;
        if (count > 64 || offset > length || (ulong)count * 4 > length - offset)
            throw new InvalidOperationException("Version list points outside the query buffer.");
        uint[] result = new uint[count];
        for (int i = 0; i < result.Length; i++) result[i] = BinaryPrimitives.ReadUInt32BigEndian(new ReadOnlySpan<byte>(values + i, 4));
        return result;
    }
}

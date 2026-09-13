using static Managed.Transport.MsQuic;
using System.Collections.Concurrent;
using Managed.Transport.Hosting;

namespace Managed.Transport.Api;

public sealed record QuicRuntimeOptions
{
    public uint ProcessorCount { get; init; }
    public QuicSettings? Settings { get; init; }
    public QuicRuntimeParameters? Parameters { get; init; }
    public int MaximumCopiedSendBytes { get; init; } = 4 * 1024 * 1024;
    public int MaximumReceiveOfferBytes { get; init; } = 1024 * 1024;
    public long ConnectionBufferBudget { get; init; } = 64L * 1024 * 1024;
    public long RuntimeBufferBudget { get; init; } = 256L * 1024 * 1024;
    public int MaximumPendingConnections { get; init; } = 32;
    public int MaximumConnectionsPerRegistration { get; init; } = 256;
}

/// <summary>Owns one translated MsQuic library instance and its registrations.</summary>
public sealed partial class QuicRuntime : IAsyncDisposable
{
    private readonly object gate = new();
    private readonly HashSet<QuicRegistration> registrations = [];
    private readonly BlockingCollection<(Action Action, TaskCompletionSource Completion)> cleanup = new();
    private readonly Thread cleanupThread;
    private nint api;
    private bool closing;
    private Task? closeTask;
    private long reservedBytes;
    private int admissions;
    private TaskCompletionSource? admissionsDrained;
    internal MsQuicHost Host { get; }
    internal QuicRuntimeOptions Options { get; }
    internal unsafe QUIC_API_TABLE* Api => (QUIC_API_TABLE*)api;
    public uint ProtocolVersion => 1;

    private QuicRuntime(QuicRuntimeOptions options)
    {
        // Validate the complete selected request before installing host state.
        var settings = options.Settings ?? new QuicSettings();
        ValidateSettings(settings);
        ValidateRuntimeParameters(options.Parameters);
        if (options.MaximumCopiedSendBytes <= 0 || options.MaximumReceiveOfferBytes <= 0 ||
            options.ConnectionBufferBudget < Math.Max(options.MaximumCopiedSendBytes, 2L * options.MaximumReceiveOfferBytes) ||
            options.RuntimeBufferBudget < options.ConnectionBufferBudget || options.MaximumPendingConnections <= 0 ||
            options.MaximumConnectionsPerRegistration < options.MaximumPendingConnections)
            throw new ArgumentOutOfRangeException(nameof(options), "Invalid bounded buffer or connection admission limits.");
        Options = options;
        Host = new MsQuicHost(options.ProcessorCount);
        cleanupThread = new Thread(CleanupLoop) { IsBackground = true, Name = "MsQuic managed API cleanup" };
        try
        {
            Host.Install();
            OpenNative(settings);
            InitializeRuntimeParameters(options.Parameters);
            cleanupThread.Start();
        }
        catch
        {
            CloseNative();
            Host.Dispose();
            cleanup.Dispose();
            throw;
        }
    }

    public static ValueTask<QuicRuntime> CreateAsync(QuicRuntimeOptions? options = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(new QuicRuntime(options ?? new()));
    }

    private static unsafe void ValidateSettings(QuicSettings settings) => _ = settings.ToNative(initialize: true);
    private unsafe void OpenNative(QuicSettings settings)
    {
        void* table = null;
        QuicError.ThrowIfFailed(MsQuic.MsQuicOpenVersion(2, &table), "MsQuicOpenVersion");
        api = (nint)table;
        QUIC_SETTINGS native = settings.ToNative(initialize: true);
        QuicError.ThrowIfFailed(Api->SetParam(null, MsQuic.QUIC_PARAM_GLOBAL_SETTINGS, (uint)sizeof(QUIC_SETTINGS), &native), "global settings");
        SetVersionOne(Api, null, MsQuic.QUIC_PARAM_GLOBAL_VERSION_SETTINGS);
    }
    internal static unsafe void SetVersionOne(QUIC_API_TABLE* api, QUIC_HANDLE* handle, uint parameter)
    {
        uint version = MsQuic.QUIC_VERSION_1_H;
        QUIC_VERSION_SETTINGS settings = new()
        {
            AcceptableVersions = &version, AcceptableVersionsLength = 1,
            OfferedVersions = &version, OfferedVersionsLength = 1,
            FullyDeployedVersions = &version, FullyDeployedVersionsLength = 1
        };
        QuicError.ThrowIfFailed(api->SetParam(handle, parameter, (uint)sizeof(QUIC_VERSION_SETTINGS), &settings), "QUIC v1 version policy");
    }
    private unsafe void CloseNative()
    {
        if (api == 0) return;
        MsQuic.MsQuicClose((void*)api);
        api = 0;
    }

    public async ValueTask<QuicRegistration> OpenRegistrationAsync(string applicationName = "dotcc-quic", CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(closing, this);
            admissions++;
        }
        try
        {
            var registration = QuicRegistration.Create(this, applicationName);
            try { lock (gate) registrations.Add(registration); }
            catch { await registration.DisposeAsync().ConfigureAwait(false); throw; }
            return registration;
        }
        finally { lock (gate) if (--admissions == 0) admissionsDrained?.TrySetResult(); }
    }
    internal void RegistrationClosed(QuicRegistration registration)
    { lock (gate) registrations.Remove(registration); }

    internal bool TryReserveBufferBytes(long bytes, out IDisposable? lease)
    {
        lease = null;
        if (bytes < 0) throw new ArgumentOutOfRangeException(nameof(bytes));
        lock (gate)
        {
            if (closing || bytes > Options.RuntimeBufferBudget - reservedBytes) return false;
            var reservation = new QuicBudgetLease(() => { lock (gate) reservedBytes -= bytes; });
            reservedBytes += bytes;
            lease = reservation;
            return true;
        }
    }

    internal Task RunCleanup(Action action)
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        cleanup.Add((action, completion));
        return completion.Task;
    }
    private void CleanupLoop()
    {
        foreach (var work in cleanup.GetConsumingEnumerable())
        {
            try { work.Action(); work.Completion.TrySetResult(); }
            catch (Exception error) { work.Completion.TrySetException(error); }
        }
    }

    public ValueTask DisposeAsync()
    {
        lock (gate)
        {
            if (closeTask != null) return new(closeTask);
            closing = true;
            Task drain = admissions == 0 ? Task.CompletedTask : (admissionsDrained ??= new(TaskCreationOptions.RunContinuationsAsynchronously)).Task;
            closeTask = Task.Run(async () =>
            {
                await drain.ConfigureAwait(false);
                QuicRegistration[] children;
                lock (gate) children = registrations.ToArray();
                await Task.WhenAll(children.Select(child => child.DisposeAsync().AsTask())).ConfigureAwait(false);
                await RunCleanup(CloseNative).ConfigureAwait(false);
                Host.Dispose();
                lock (gate) if (reservedBytes != 0) throw new InvalidOperationException("QUIC buffer reservations did not drain.");
                cleanup.CompleteAdding();
                cleanupThread.Join();
                cleanup.Dispose();
            });
            return new(closeTask);
        }
    }
}

using System;
using Managed.Transport;

namespace Managed.Transport.Hosting;

public sealed unsafe partial class MsQuicHost : IDisposable
{
    private static readonly object installationGate = new();
    private static MsQuicHost? installedHost;
    private bool disposed;
    internal uint ProcessorCount { get; }
    internal ulong TotalMemory { get; }

    public MsQuicHost(uint processorCount = 0)
    {
        ProcessorCount = processorCount == 0 ? (uint)Math.Clamp(Environment.ProcessorCount, 1, ushort.MaxValue) : processorCount;
        if (ProcessorCount > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(processorCount));
        long available = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        if (available <= 0) throw new InvalidOperationException("The BCL did not report a usable memory budget.");
        TotalMemory = (ulong)available;
        InitializeResources();
    }

    internal MSQUIC_HOST_TABLE CreateTable()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        MSQUIC_HOST_TABLE table = default;
        table.Size = (uint)sizeof(MSQUIC_HOST_TABLE);
        table.Version = 1;
        table.Context = ContextPointer;
        table.ProcessorCount = ProcessorCount;
        table.TotalMemory = TotalMemory;
        RegisterPlatform(ref table);
        RegisterCrypto(ref table);
        RegisterTls(ref table);
        RegisterDatapath(ref table);
        return table;
    }

    // The owning transport facade controls upstream library loading and shutdown.
    // Installation itself does not open a registration or initialize transport.
    internal void Install()
    {
        lock (installationGate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (installedHost != null) throw new InvalidOperationException("A host is already installed in this generated library.");
            MSQUIC_HOST_TABLE table = CreateTable();
            uint status = MsQuic.MsQuicHostInstall(&table);
            if (status != Status.Success) throw new InvalidOperationException("Host table installation failed: " + status);
            installedHost = this;
        }
    }

    public void Dispose()
    {
        lock (installationGate)
        {
            if (disposed) return;
            // Never free a live context. Upstream close must finish all callbacks,
            // workers and pending I/O before the host can be uninstalled.
            if (OutstandingResources != 0 || OutstandingPlatformAllocations != 0) throw new InvalidOperationException("Close all host resources and platform allocations before disposing the host.");
            if (ReferenceEquals(installedHost, this))
            {
                uint status = MsQuic.MsQuicHostUninstall();
                if (status != Status.Success) throw new InvalidOperationException("The upstream library must unload before its host is disposed: " + status);
                installedHost = null;
            }
            ReleaseContext();
            disposed = true;
        }
    }
}

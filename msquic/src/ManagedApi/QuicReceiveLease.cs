using static Managed.Transport.MsQuic;
using System;
using System.Collections.Generic;
using System.Threading;
using Managed.Transport;

namespace Managed.Transport.Api;

/// <summary>A deferred receive offer with managed copies of the data. Completing the offer releases native receive ownership; previously obtained buffer views remain memory-safe.</summary>
public sealed class QuicReceiveLease : IDisposable
{
    private readonly QuicStream owner;
    private readonly IReadOnlyList<ReadOnlyMemory<byte>> buffers;
    private int completing;
    private readonly IDisposable budget;
    public ulong AbsoluteOffset { get; }
    public ulong Length { get; }
    public bool HasFin { get; }
    public IReadOnlyList<ReadOnlyMemory<byte>> Buffers { get { ThrowIfCompleted(); return buffers; } }

    internal unsafe QuicReceiveLease(QuicStream owner, ulong offset, ulong length, QUIC_BUFFER* descriptors, uint count, bool fin, IDisposable budget)
    {
        this.owner = owner; this.budget = budget; AbsoluteOffset = offset; Length = length; HasFin = fin;
        if (count != 0 && descriptors == null) throw new InvalidOperationException("Null receive descriptors");
        int totalLength = checked((int)length);
        ulong total = 0;
        for (uint i = 0; i < count; i++)
        {
            uint size = descriptors[i].Length;
            if (size != 0 && descriptors[i].Buffer == null) throw new InvalidOperationException("Null receive payload");
            total = checked(total + size);
        }
        if (total != length) throw new InvalidOperationException("Receive descriptor length mismatch");
        var payload = GC.AllocateUninitializedArray<byte>(totalLength);
        int written = 0;
        for (uint i = 0; i < count; i++)
        {
            int size = checked((int)descriptors[i].Length);
            new ReadOnlySpan<byte>(descriptors[i].Buffer, size).CopyTo(payload.AsSpan(written));
            written += size;
        }
        // Coalescing descriptor segments bounds metadata as well as payload.
        buffers = Array.AsReadOnly(new ReadOnlyMemory<byte>[] { payload });
    }

    /// <summary>Consumes a prefix and releases the entire offer. A suffix is offered again after receive resumes.</summary>
    public void Complete(ulong consumed, bool resume = true)
    {
        if (consumed > Length) throw new ArgumentOutOfRangeException(nameof(consumed));
        owner.CompleteReceive(this, consumed, resume, idempotent: false);
    }

    /// <summary>Releases the borrow without consuming bytes and leaves receive paused.</summary>
    public void Dispose() => owner.CompleteReceive(this, 0, resume: false, idempotent: true);

    internal void ReleaseBudget() => budget.Dispose();
    internal bool ClaimCompletion() => Interlocked.CompareExchange(ref completing, 1, 0) == 0;
    private void ThrowIfCompleted() => ObjectDisposedException.ThrowIf(Volatile.Read(ref completing) != 0, this);
    internal int CopyTo(Span<byte> destination)
    {
        ThrowIfCompleted();
        int written = 0;
        foreach (var memory in buffers)
        {
            int count = Math.Min(memory.Length, destination.Length - written);
            memory.Span[..count].CopyTo(destination[written..]); written += count;
            if (written == destination.Length) break;
        }
        return written;
    }

}

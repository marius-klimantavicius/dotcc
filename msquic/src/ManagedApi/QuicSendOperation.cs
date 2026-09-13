using static Managed.Transport.MsQuic;
using System;
using System.Threading;
using System.Threading.Tasks;
using Managed.Transport;

namespace Managed.Transport.Api;

// The application input is copied before admission. This owner remains in the
// stream's registry through SEND_COMPLETE, independent of caller cancellation.
internal sealed class QuicSendOperation
{
    private static long sequence;
    private readonly byte[] payload;
    private readonly QUIC_BUFFER[] descriptors;
    private int completed;
    private readonly IDisposable budget;
    internal readonly nint Token;
    internal readonly TaskCompletionSource Completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal unsafe QuicSendOperation(ReadOnlyMemory<byte> source, IDisposable budget)
    {
        this.budget = budget;
        payload = GC.AllocateUninitializedArray<byte>(Math.Max(1, source.Length), pinned: true);
        source.Span.CopyTo(payload);
        descriptors = GC.AllocateArray<QUIC_BUFFER>(1, pinned: true);
        fixed (byte* pointer = payload) descriptors[0] = new QUIC_BUFFER { Buffer = pointer, Length = (uint)source.Length };
        long value = Interlocked.Increment(ref sequence);
        if (value <= 0) throw new InvalidOperationException("Send token space exhausted");
        Token = checked((nint)value);
    }
    internal unsafe QUIC_BUFFER* Buffers { get { fixed (QUIC_BUFFER* value = descriptors) return value; } }
    internal bool Finish(Exception? error)
    {
        if (Interlocked.Exchange(ref completed, 1) != 0) return false;
        budget.Dispose();
        if (error == null) Completion.TrySetResult(); else Completion.TrySetException(error);
        return true;
    }
}

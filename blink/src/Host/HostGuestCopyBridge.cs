using global::System;
using Managed.Emulation.Host;

namespace Managed.Emulation;

#if BLINK_FULL_CORE
public partial class BlinkCore
#else
public static partial class Blink
#endif
{
    private static IHostGuestMemoryCopies? GuestCopies
    {
        get
        {
#if BLINK_GUEST_THREAD_COPIES
            return guestThreads as IHostGuestMemoryCopies;
#else
            return null;
#endif
        }
    }

    // Quota follows ownership of the async staging array through GetResult and
    // final copyback. This value scope adds no per-copy observer allocation.
    private struct GuestIoBuffer : IDisposable
    {
        private IHostGuestMemoryCopies? owner;
        private readonly ulong charge;
        internal readonly byte[] Bytes;
        internal GuestIoBuffer(int count)
        {
            if ((uint)count > IoChunk) throw new ArgumentOutOfRangeException(nameof(count));
            owner = GuestCopies;
            charge = checked((ulong)count + 32);
            if (owner != null && !owner.TryReserveGuestCopyBytes(charge)) throw new OutOfMemoryException();
            try { Bytes = new byte[count]; }
            catch { owner?.ReleaseGuestCopyBytes(charge); owner = null; throw; }
        }
        public void Dispose()
        {
            var admitted = owner; owner = null;
            admitted?.ReleaseGuestCopyBytes(charge);
        }
    }

    // All actual-use scopes are managed finally regions. The surrounding C
    // PAGE_LOCK or CPU passive pin supplies pointer lifetime independently.
    private static unsafe void CopyGuestRead(void* destination, void* source, int count)
    {
        if ((uint)count > 4096) throw new ArgumentOutOfRangeException(nameof(count));
        if (count == 0) return;
        var owner = GuestCopies;
        owner?.BeginGuestRead();
        try { new ReadOnlySpan<byte>(source, count).CopyTo(new Span<byte>(destination, count)); }
        finally { owner?.EndGuestRead(); }
    }
    private static unsafe void CopyGuestWrite(void* destination, void* source, int count)
    {
        if ((uint)count > 4096) throw new ArgumentOutOfRangeException(nameof(count));
        if (count == 0) return;
        var owner = GuestCopies;
        owner?.BeginGuestWrite((nint)destination, count);
        try { new ReadOnlySpan<byte>(source, count).CopyTo(new Span<byte>(destination, count)); }
        finally { owner?.EndGuestWrite(); }
    }
    private static unsafe void SnapshotGuestBytes(void* source, Span<byte> destination)
    {
        for (int offset = 0; offset < destination.Length;)
        {
            byte* pointer = (byte*)source + offset;
            int count = Math.Min(4096 - (int)((nuint)pointer & 4095), destination.Length - offset);
            fixed (byte* target = destination.Slice(offset, count)) CopyGuestRead(target, pointer, count);
            offset += count;
        }
    }
    private static unsafe void StoreGuestBytes(void* destination, ReadOnlySpan<byte> source)
    {
        for (int offset = 0; offset < source.Length;)
        {
            byte* pointer = (byte*)destination + offset;
            int count = Math.Min(4096 - (int)((nuint)pointer & 4095), source.Length - offset);
            fixed (byte* from = source.Slice(offset, count)) CopyGuestWrite(pointer, from, count);
            offset += count;
        }
    }

    private static unsafe bool ReadGuestPath(byte* path, out string name)
    {
        name = "";
        var owner = GuestCopies;
        owner?.BeginGuestRead();
        try
        {
            int length = 0;
            while (length < PathLimit && path[length] != 0) ++length;
            if (length == PathLimit) { IoError(36); return false; }
            try { name = PathEncoding.GetString(new ReadOnlySpan<byte>(path, length)); }
            catch (global::System.Text.DecoderFallbackException) { IoError(22); return false; }
            return true;
        }
        finally { owner?.EndGuestRead(); }
    }

#if BLINK_GUEST_THREAD_COPIES
#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe void blink_host_guest_read_copy(BlinkCore program, Machine* machine, void* destination, void* source, ulong count)
#else
    public static unsafe void blink_host_guest_read_copy(Machine* machine, void* destination, void* source, ulong count)
#endif
    {
#if DOTCC_INSTANCE_FOR_HOST
        _ = GuestOwner(program);
#endif
        CopyGuestRead(destination, source, checked((int)count));
    }
#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe void blink_host_guest_write_copy(BlinkCore program, Machine* machine, void* destination, void* source, ulong count)
#else
    public static unsafe void blink_host_guest_write_copy(Machine* machine, void* destination, void* source, ulong count)
#endif
    {
#if DOTCC_INSTANCE_FOR_HOST
        _ = GuestOwner(program);
#endif
        CopyGuestWrite(destination, source, checked((int)count));
    }

#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe ulong blink_host_guest_string_copy(BlinkCore program, Machine* machine, byte* destination, byte* source, ulong count)
#else
    public static unsafe ulong blink_host_guest_string_copy(Machine* machine, byte* destination, byte* source, ulong count)
#endif
    {
#if DOTCC_INSTANCE_FOR_HOST
        _ = GuestOwner(program);
#endif
        if (count > 4096) throw new ArgumentOutOfRangeException(nameof(count));
        var owner = GuestCopies;
        owner?.BeginGuestRead();
        try
        {
            for (ulong i = 0; i < count; ++i)
            {
                byte value = source[i]; destination[i] = value;
                if (value == 0) return i + 1;
            }
            return 0;
        }
        finally { owner?.EndGuestRead(); }
    }
#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe void blink_host_guest_store32(BlinkCore program, Machine* machine, byte* destination, uint value, int sequential)
#else
    public static unsafe void blink_host_guest_store32(Machine* machine, byte* destination, uint value, int sequential)
#endif
    {
#if DOTCC_INSTANCE_FOR_HOST
        _ = GuestOwner(program);
#endif
        var owner = GuestCopies;
        owner?.BeginGuestWrite((nint)destination, 4);
        try
        {
            // Keep the selected typed C store and order, including cleanup
            // callers whose set_tid_address input has no alignment precheck.
#if DOTCC_INSTANCE_FOR_HOST
            program.BlinkGuestTidStoreRaw(machine, destination, value, sequential);
#else
            BlinkGuestTidStoreRaw(machine, destination, value, sequential);
#endif
        }
        finally { owner?.EndGuestWrite(); }
    }
#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe int blink_host_guest_futex_enqueue(BlinkCore program, Machine* machine, byte* memory, uint expect, ulong key, int kind, uint mask, void* waiter, void** result)
#else
    public static unsafe int blink_host_guest_futex_enqueue(Machine* machine, byte* memory, uint expect, ulong key, int kind, uint mask, void* waiter, void** result)
#endif
    {
#if DOTCC_INSTANCE_FOR_HOST
        _ = GuestOwner(program);
#endif
        var owner = GuestCopies;
        // The caller allocated one waiter before admission. A failed admission
        // cannot leave that unregistered waiter outside either C cleanup list.
        try { owner?.BeginGuestRead(); }
        catch { Libc.free(waiter); throw; }
        try
        {
#if DOTCC_INSTANCE_FOR_HOST
            return program.BlinkGuestFutexEnqueue(machine, memory, expect, key, kind, mask, waiter, result);
#else
            return BlinkGuestFutexEnqueue(machine, memory, expect, key, kind, mask, waiter, result);
#endif
        }
        finally { owner?.EndGuestRead(); }
    }

#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe int blink_host_guest_consume_signal(BlinkCore program, Machine* machine, int* delivered, CBool* restart)
#else
    public static unsafe int blink_host_guest_consume_signal(Machine* machine, int* delivered, CBool* restart)
#endif
    {
#if DOTCC_INSTANCE_FOR_HOST
        _ = GuestOwner(program);
#endif
        var owner = GuestCopies;
        owner?.BeginGuestRead(); // Before BlinkGuestConsumeSignal takes sig_lock.
        try
        {
#if DOTCC_INSTANCE_FOR_HOST
            return program.BlinkGuestConsumeSignal(machine, delivered, restart);
#else
            return BlinkGuestConsumeSignal(machine, delivered, restart);
#endif
        }
        finally { owner?.EndGuestRead(); }
    }

#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe void blink_host_guest_deliver_signal_user(BlinkCore program, Machine* machine, int signal, int code)
#else
    public static unsafe void blink_host_guest_deliver_signal_user(Machine* machine, int signal, int code)
#endif
    {
#if DOTCC_INSTANCE_FOR_HOST
        _ = GuestOwner(program);
#endif
        var owner = GuestCopies;
        owner?.BeginGuestRead();
        try
        {
#if DOTCC_INSTANCE_FOR_HOST
            program.BlinkGuestDeliverSignalToUser(machine, signal, code);
#else
            BlinkGuestDeliverSignalToUser(machine, signal, code);
#endif
        }
        finally { owner?.EndGuestRead(); }
    }

#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe long blink_host_guest_handle_sigpipe(BlinkCore program, Machine* machine, long result, int flags)
#else
    public static unsafe long blink_host_guest_handle_sigpipe(Machine* machine, long result, int flags)
#endif
    {
#if DOTCC_INSTANCE_FOR_HOST
        _ = GuestOwner(program);
#endif
        var owner = GuestCopies;
        owner?.BeginGuestRead();
        try
        {
#if DOTCC_INSTANCE_FOR_HOST
            return program.BlinkGuestHandleSigpipe(machine, result, flags);
#else
            return BlinkGuestHandleSigpipe(machine, result, flags);
#endif
        }
        finally { owner?.EndGuestRead(); }
    }

#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe int blink_host_guest_tkill_self(BlinkCore program, Machine* machine, int signal)
#else
    public static unsafe int blink_host_guest_tkill_self(Machine* machine, int signal)
#endif
    {
#if DOTCC_INSTANCE_FOR_HOST
        _ = GuestOwner(program);
#endif
        var owner = GuestCopies;
        owner?.BeginGuestRead();
        try
        {
#if DOTCC_INSTANCE_FOR_HOST
            return program.BlinkGuestTkillSelf(machine, signal);
#else
            return BlinkGuestTkillSelf(machine, signal);
#endif
        }
        finally { owner?.EndGuestRead(); }
    }

#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe int blink_host_guest_sigaction(BlinkCore program, Machine* machine, int signal, long action, long oldAction, ulong signalSetSize)
#else
    public static unsafe int blink_host_guest_sigaction(Machine* machine, int signal, long action, long oldAction, ulong signalSetSize)
#endif
    {
#if DOTCC_INSTANCE_FOR_HOST
        _ = GuestOwner(program);
#endif
        // The finite sigaction implementation copies the old action while
        // holding sig_lock. Admit BEFORE that lock; its nested actual copies
        // must not wait behind a native reservation draining this envelope.
        var owner = GuestCopies;
        owner?.BeginGuestRead();
        try
        {
#if DOTCC_INSTANCE_FOR_HOST
            return program.BlinkGuestSigaction(machine, signal, action, oldAction, signalSetSize);
#else
            return BlinkGuestSigaction(machine, signal, action, oldAction, signalSetSize);
#endif
        }
        finally { owner?.EndGuestRead(); }
    }
#endif
}

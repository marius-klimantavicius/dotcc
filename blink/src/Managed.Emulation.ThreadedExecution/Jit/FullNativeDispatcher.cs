using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace Managed.Emulation.Execution;

// Invocation-private authenticated table. All pointers and executable ranges
// are protected by GuestBlockJit's gate and permanent static-code read leases.
// No mapping lock is acquired here. Table misses return to managed translation.
internal sealed unsafe class FullNativeDispatcher : IDisposable
{
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    private struct Target
    {
        [FieldOffset(0)] internal ulong Ip;
        [FieldOffset(8)] internal nint Entry;
        [FieldOffset(16)] internal ulong Owner;
        [FieldOffset(24)] internal ulong Generation;
    }
    [StructLayout(LayoutKind.Explicit, Size = 80)]
    private struct Context
    {
        [FieldOffset(0)] internal ulong Abi;
        [FieldOffset(8)] internal Target* Table;
        [FieldOffset(16)] internal uint BucketMask;
        [FieldOffset(24)] internal nint Dispatcher;
        [FieldOffset(32)] internal ulong Executed;
        [FieldOffset(40)] internal uint Remaining;
        [FieldOffset(44)] internal uint Blocks;
        [FieldOffset(56)] internal ulong Owner;
        [FieldOffset(64)] internal ulong Generation;
    }
    internal const ulong Abi = 0x00504a54494c4601UL; // FLITJP, version 1
    private readonly int count;
    private readonly NativeCodeAllocation code;
    private Context* context;
    internal long Charge => 128 + sizeof(Context) + count * sizeof(Target) + code.AllocatedBytes;
    internal nint Handle => (nint)context;
    internal ulong Executed => context->Executed;
    internal uint Blocks => context->Blocks;

    internal FullNativeDispatcher(int count)
    {
        if (count < 4 || (count & (count - 1)) != 0) throw new ArgumentOutOfRangeException(nameof(count));
        this.count = count;
        if (!NativeCodeAllocation.TryCreate(Emit(), out var allocation, out string reason))
            throw new PlatformNotSupportedException("Full-JIT native dispatcher: " + reason);
        code = allocation!;
        context = (Context*)NativeMemory.AllocZeroed((nuint)sizeof(Context));
        if (context == null) { code.Dispose(); throw new OutOfMemoryException(); }
        context->Table = (Target*)NativeMemory.AllocZeroed((nuint)(count * sizeof(Target)));
        if (context->Table == null) { NativeMemory.Free(context); context = null; code.Dispose(); throw new OutOfMemoryException(); }
        context->Abi = Abi; context->BucketMask = (uint)(count / 4 - 1);
        // The bootstrap establishes R10/R11; linked leaves enter the second
        // ENDBR64 at offset 11, preserving those private volatile registers.
        context->Dispatcher = code.Entry + 11;
    }

    internal void Begin(ulong owner)
    {
        context->Owner = owner;
        context->Generation = checked(context->Generation + 1);
        context->Executed = 0; context->Remaining = 32; context->Blocks = 0;
        NativeMemory.Clear(context->Table, (nuint)(count * sizeof(Target)));
    }

    internal void Set(int slot, ulong ip, nint entry)
    {
        if ((uint)slot >= count || entry == 0) throw new ArgumentOutOfRangeException(nameof(slot));
        context->Table[slot] = new() { Ip = ip, Entry = entry, Owner = context->Owner, Generation = context->Generation };
    }

    internal void Run(JitFrame* frame)
    {
        if (context->Abi != Abi || context->Remaining != 32 || context->Executed != 0)
            throw new InvalidOperationException("Full-JIT dispatcher context ABI/state differs.");
        frame->OwnerHandle = Handle; frame->Executed = 0;
        ((delegate* unmanaged<JitFrame*, void>)code.Entry)(frame);
        if (context->Blocks > 32 || frame->Executed != context->Executed || frame->Executed > 32 ||
            context->Remaining + frame->Executed != 32)
            throw new InvalidOperationException("Full-JIT native dispatcher exceeded its aggregate cap.");
    }

    public void Dispose()
    {
        if (context == null) return;
        // Owner gate and joined workers precede retirement; no pointer escapes.
        NativeMemory.Free(context->Table); NativeMemory.Free(context); context = null;
        code.Dispose();
    }

    private static byte[] Emit()
    {
        // Small independent assembler; every branch target is in this leaf.
        var bytes = new List<byte>(256);
        var misses = new List<int>();
        void B(params byte[] values) => bytes.AddRange(values);
        void U32(uint value) { Span<byte> s = stackalloc byte[4]; BinaryPrimitives.WriteUInt32LittleEndian(s, value); foreach (byte b in s) bytes.Add(b); }
        int J(byte condition) { B(0x0f, condition); int at = bytes.Count; U32(0); return at; }
        void Patch(int at, int target) { uint offset = unchecked((uint)(target - at - 4)); for (int i = 0; i < 4; ++i) bytes[at + i] = (byte)(offset >> (8 * i)); }
        B(0xf3, 0x0f, 0x1e, 0xfa, 0x49, 0x89, OperatingSystem.IsWindows() ? (byte)0xca : (byte)0xfa);
        B(0x4d, 0x8b, 0x5a, 0x08); // r10=frame; r11=machine
        B(0xf3, 0x0f, 0x1e, 0xfa); // internal tail-jump entry
        B(0x4d, 0x8b, 0x42, 0x40); // r8=context
        B(0x41, 0x83, 0x78, 0x28, 0); misses.Add(J(0x84)); // remaining==0
        B(0x49, 0x8b, 0x03, 0x48, 0x89, 0xc1, 0x48, 0xc1, 0xe9, 12, 0x48, 0x31, 0xc1);
        B(0x48, 0x89, 0xc2, 0x48, 0xc1, 0xea, 24, 0x48, 0x31, 0xd1);
        B(0x41, 0x23, 0x48, 0x10, 0x48, 0xc1, 0xe1, 7, 0x49, 0x03, 0x48, 8); // bucket of 4x32-byte targets
        for (int way = 0; way < 4; ++way)
        {
            List<int> next = [];
            B(0x48, 0x39, 0x01); next.Add(J(0x85)); // actual IP
            B(0x48, 0x83, 0x79, 8, 0); next.Add(J(0x84)); // populated code pointer
            B(0x48, 0x8b, 0x51, 16, 0x49, 0x3b, 0x50, 56); next.Add(J(0x85)); // owner
            B(0x48, 0x8b, 0x51, 24, 0x49, 0x3b, 0x50, 64); next.Add(J(0x85)); // invocation generation
            B(0x41, 0x8b, 0x50, 40, 0x41, 0x89, 0x52, 60); // shared remaining cap
            B(0x41, 0xff, 0x40, 44, 0x48, 0x8b, 0x41, 8); // ++blocks; rax=authenticated target
            B(0x4c, 0x89, OperatingSystem.IsWindows() ? (byte)0xd1 : (byte)0xd7); // restore RCX/RDI=frame for target prologue
            B(0xff, 0xe0); // tail-jump authenticated target
            foreach (int at in next) Patch(at, bytes.Count);
            B(0x48, 0x83, 0xc1, 32);
        }
        foreach (int at in misses) Patch(at, bytes.Count);
        B(0x49, 0x8b, 0x40, 32, 0x49, 0x89, 0x42, 48); // aggregate executed
        B(0x49, 0x8b, 0x03, 0x49, 0x89, 0x42, 72); // next guest IP
        B(0x41, 0xc7, 0x42, 56); U32((uint)JitExit.Continue); B(0xc3);
        return bytes.ToArray();
    }
}

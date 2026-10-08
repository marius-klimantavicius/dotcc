#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Edg.Runtime;

// Writable argv storage owned by one executable invocation. Keep this owner
// alive through normal-exit callbacks; changes to argv entries do not transfer
// ownership of the original allocation or of pointers assigned by source code.
public sealed unsafe class ProgramArguments : IDisposable
{
    private static readonly UTF8Encoding encoding = new(false, true);
    private nint storage;
    public int Count { get; }
    public sbyte** Pointer
    {
        get
        {
            nint value = Volatile.Read(ref storage);
            ObjectDisposedException.ThrowIf(value == 0, this);
            return (sbyte**)value;
        }
    }

    public static ProgramArguments FromCommandLine() => new(Environment.GetCommandLineArgs());

    // Snapshot caller-owned references before sizing native storage. Strings
    // themselves are immutable; concurrent replacement of an input array entry
    // must not turn the second encoding pass into a native buffer overrun.
    public ProgramArguments(ReadOnlySpan<string> arguments) : this(arguments.ToArray()) { }

    private ProgramArguments(string[] arguments)
    {
        Count = arguments.Length;
        nuint tableBytes = checked(((nuint)Count + 1) * (nuint)sizeof(void*));
        nuint bytes = tableBytes;
        foreach (string argument in arguments)
        {
            ArgumentNullException.ThrowIfNull(argument);
            if (argument.AsSpan().Contains('\0'))
                throw new ArgumentException("A C program argument cannot contain an embedded NUL.", nameof(arguments));
            bytes = checked(bytes + (nuint)encoding.GetByteCount(argument) + 1);
        }
        byte* block = (byte*)NativeMemory.Alloc(bytes);
        if (block == null) throw new OutOfMemoryException();
        try
        {
            sbyte** vector = (sbyte**)block;
            byte* next = block + tableBytes;
            for (int index = 0; index < Count; ++index)
            {
                string argument = arguments[index];
                int length = encoding.GetByteCount(argument);
                vector[index] = (sbyte*)next;
                encoding.GetBytes(argument.AsSpan(), new Span<byte>(next, length));
                next[length] = 0;
                next += (nuint)length + 1;
            }
            vector[Count] = null;
            storage = (nint)block;
        }
        catch { NativeMemory.Free(block); throw; }
    }

    ~ProgramArguments() => Release();
    private void Release() => NativeMemory.Free((void*)Interlocked.Exchange(ref storage, 0));
    public void Dispose() { Release(); GC.SuppressFinalize(this); }
}

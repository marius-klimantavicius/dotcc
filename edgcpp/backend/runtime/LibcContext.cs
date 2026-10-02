#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Edg.Runtime;

/// <summary>The audited first libc slice for the EDG LP64 ABI. Stateful data is
/// owner/thread specific. Allocation belongs to the generated program's heap.</summary>
public sealed unsafe class LibcContext : IDisposable
{
    // Memory primitives follow DotCC.Libc/MemorySizeLib.cs. Byte comparisons and
    // tokenization follow the unsigned-character contracts used by StringLib.cs.
    // This independent adapter deliberately fixes the legacy int strlen ABI and
    // does not import the legacy singleton context or a second heap implementation.
    private sealed class ThreadState
    {
        internal readonly int[] Error = GC.AllocateArray<int>(1, pinned: true);
        internal nint TokenCursor;
    }
    private readonly ThreadLocal<ThreadState> threads = new(() => new ThreadState());
    private int disposed;
    private ThreadState Current
    {
        get { ObjectDisposedException.ThrowIf(Volatile.Read(ref disposed) != 0, this); return threads.Value ??= new ThreadState(); }
    }
    public int* ErrnoLocation() { fixed (int* p = Current.Error) return p; }
    public void SetErrno(int error) => Current.Error[0] = error;
    // Generated lifecycle leases exclude simultaneous use/disposal. ReleaseThread
    // is called after translated TLS destructors, which may still use errno.
    public void ReleaseThread() { ObjectDisposedException.ThrowIf(Volatile.Read(ref disposed) != 0, this); threads.Value = null!; }
    public void Dispose() { if (Interlocked.Exchange(ref disposed, 1) == 0) threads.Dispose(); }

    public static void* Memcpy(void* destination, void* source, ulong count)
    {
        if (count != 0) Buffer.MemoryCopy(source, destination, count, count);
        return destination;
    }
    public static void* Memmove(void* destination, void* source, ulong count)
    {
        // Buffer.MemoryCopy preserves source bytes when the regions overlap.
        if (count != 0) Buffer.MemoryCopy(source, destination, count, count);
        return destination;
    }
    public static void* Memset(void* destination, int value, ulong count)
    {
        if (count != 0) NativeMemory.Fill(destination, checked((nuint)count), unchecked((byte)value));
        return destination;
    }
    public static int Memcmp(void* left, void* right, ulong count)
    {
        byte* a = (byte*)left; byte* b = (byte*)right;
        while (count != 0)
        {
            int length = count > int.MaxValue ? int.MaxValue : (int)count;
            int comparison = new ReadOnlySpan<byte>(a, length).SequenceCompareTo(new ReadOnlySpan<byte>(b, length));
            if (comparison != 0) return comparison;
            a += length; b += length; count -= (uint)length;
        }
        return 0;
    }
    public static void* Memchr(void* source, int value, ulong count)
    {
        byte* p = (byte*)source; byte needle = unchecked((byte)value);
        while (count != 0)
        {
            int length = count > int.MaxValue ? int.MaxValue : (int)count;
            int index = new ReadOnlySpan<byte>(p, length).IndexOf(needle);
            if (index >= 0) return p + index;
            p += length; count -= (uint)length;
        }
        return null;
    }
    public static ulong Strlen(sbyte* source)
    {
        sbyte* p = source; while (*p != 0) ++p; return (ulong)(p - source);
    }
    public static ulong Strnlen(sbyte* source, ulong maximum)
    {
        ulong n = 0; while (n < maximum && source[n] != 0) ++n; return n;
    }
    public static int Strcmp(sbyte* left, sbyte* right)
    {
        byte* a = (byte*)left; byte* b = (byte*)right;
        while (*a != 0 && *a == *b) { ++a; ++b; }
        return *a - *b;
    }
    public static int Strncmp(sbyte* left, sbyte* right, ulong count)
    {
        byte* a = (byte*)left; byte* b = (byte*)right;
        while (count != 0)
        {
            if (*a != *b || *a == 0) return *a - *b;
            ++a; ++b; --count;
        }
        return 0;
    }
    public static sbyte* Strcpy(sbyte* destination, sbyte* source)
    {
        sbyte* p = destination; do { *p++ = *source; } while (*source++ != 0); return destination;
    }
    public static sbyte* Strncpy(sbyte* destination, sbyte* source, ulong count)
    {
        ulong copied = 0;
        while (copied < count && source[copied] != 0) { destination[copied] = source[copied]; ++copied; }
        if (copied < count) NativeMemory.Clear(destination + copied, checked((nuint)(count - copied)));
        return destination;
    }
    public static sbyte* Strcat(sbyte* destination, sbyte* source)
    {
        Strcpy(destination + Strlen(destination), source); return destination;
    }
    public static sbyte* Strncat(sbyte* destination, sbyte* source, ulong count)
    {
        sbyte* p = destination + Strlen(destination);
        while (count != 0 && *source != 0) { *p++ = *source++; --count; }
        *p = 0; return destination;
    }
    public static sbyte* Strchr(sbyte* source, int value)
    {
        byte needle = unchecked((byte)value);
        do { if ((byte)*source == needle) return source; } while (*source++ != 0);
        return null;
    }
    public static sbyte* Strrchr(sbyte* source, int value)
    {
        byte needle = unchecked((byte)value); sbyte* found = null;
        do { if ((byte)*source == needle) found = source; } while (*source++ != 0);
        return found;
    }
    public static sbyte* Strstr(sbyte* haystack, sbyte* needle)
    {
        if (*needle == 0) return haystack;
        for (; *haystack != 0; ++haystack)
        {
            sbyte* a = haystack; sbyte* b = needle;
            while (*b != 0 && *a == *b) { ++a; ++b; }
            if (*b == 0) return haystack;
        }
        return null;
    }
    public static ulong Strspn(sbyte* source, sbyte* accept)
    {
        ulong n = 0; while (source[n] != 0 && Strchr(accept, source[n]) != null) ++n; return n;
    }
    public static ulong Strcspn(sbyte* source, sbyte* reject)
    {
        ulong n = 0; while (source[n] != 0 && Strchr(reject, source[n]) == null) ++n; return n;
    }
    public static sbyte* Strpbrk(sbyte* source, sbyte* accept)
    {
        source += Strcspn(source, accept); return *source == 0 ? null : source;
    }
    public static sbyte* StrtokR(sbyte* source, sbyte* delimiters, sbyte** cursor)
    {
        if (source == null) source = *cursor;
        if (source == null) return null;
        source += Strspn(source, delimiters);
        if (*source == 0) { *cursor = source; return null; }
        sbyte* end = source + Strcspn(source, delimiters);
        *cursor = *end == 0 ? end : end + 1;
        if (*end != 0) *end = 0;
        return source;
    }
    public sbyte* Strtok(sbyte* source, sbyte* delimiters)
    {
        var state = Current; sbyte* cursor = (sbyte*)state.TokenCursor;
        sbyte* token = StrtokR(source, delimiters, &cursor); state.TokenCursor = (nint)cursor; return token;
    }
    public double Sqrt(double value) { if (value < 0) SetErrno(33); return Math.Sqrt(value); }
    public float SqrtF(float value) { if (value < 0) SetErrno(33); return MathF.Sqrt(value); }
}

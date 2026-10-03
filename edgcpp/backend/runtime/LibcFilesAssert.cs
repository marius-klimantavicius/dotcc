#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
namespace Edg.Runtime;
public sealed unsafe partial class LibcFiles
{
    // Internal diagnostics bypass public byte/wide orientation, like musl's
    // assertion implementation. The addressable owner stderr is authoritative.
    [DoesNotReturn]
    public void AssertFail(sbyte* expression, sbyte* file, int line, sbyte* function)
    {
        try { WriteAssertion(expression, file, line, function); }
        catch { LibcProcess.Abort(); }
        LibcProcess.Abort();
    }
    internal void WriteAssertion(sbyte* expression, sbyte* file, int line, sbyte* function)
    {
        var entry = Find(Stderr);
        if (entry is null) return;
        lock (entry.Gate)
        {
            AssertionBytes(entry, "Assertion failed: "u8);
            AssertionText(entry, expression);
            AssertionBytes(entry, " ("u8); AssertionText(entry, file);
            AssertionBytes(entry, ": "u8); AssertionText(entry, function);
            AssertionBytes(entry, ": "u8);
            Span<byte> digits = stackalloc byte[11];
            uint number = line < 0 ? unchecked((uint)-(long)line) : (uint)line;
            int start = digits.Length;
            do { digits[--start] = (byte)('0' + number % 10); number /= 10; } while (number != 0);
            if (line < 0) digits[--start] = (byte)'-';
            AssertionBytes(entry, digits[start..]); AssertionBytes(entry, ")\n"u8);
            FlushOutput(entry);
        }
    }
    private void AssertionBytes(Entry entry, ReadOnlySpan<byte> bytes)
    { fixed (byte* p = bytes) WriteBytes(p, 1, (ulong)bytes.Length, entry); }
    private void AssertionText(Entry entry, sbyte* text)
    {
        if (text == null) { AssertionBytes(entry, "?"u8); return; }
        ulong size = 0; while (text[size] != 0) ++size;
        if (size != 0) WriteBytes(text, 1, size, entry);
    }
}

using System;
using System.Text;
using Edg.Runtime;

static unsafe class ArgumentProbe
{
    static void Check(bool condition, string label)
    {
        if (!condition) throw new Exception(label);
        Console.WriteLine(label);
    }
    static void Main()
    {
        string[] values = ["program", "", "a b", "žąsis", "🦆"];
        using var arguments = new ProgramArguments(values);
        sbyte** vector = arguments.Pointer;
        Check(arguments.Count == values.Length && vector[values.Length] == null, "argc and sentinel");
        Check((nuint)vector % (nuint)sizeof(void*) == 0, "aligned pointer table");
        for (int index = 0; index < values.Length; ++index)
        {
            byte[] expected = Encoding.UTF8.GetBytes(values[index]);
            Check(new ReadOnlySpan<byte>(vector[index], expected.Length).SequenceEqual(expected) &&
                  vector[index][expected.Length] == 0, "UTF8 argument " + index);
        }
        vector[0][0] = (sbyte)'P';
        vector[2] = vector[0];
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        Check(vector == arguments.Pointer && vector[2][0] == 'P', "writable and stable across GC");
        arguments.Dispose(); arguments.Dispose();
        bool disposed = false;
        try { _ = arguments.Pointer; } catch (ObjectDisposedException) { disposed = true; }
        Check(disposed, "disposed storage access rejected");
        using var empty = new ProgramArguments([]);
        Check(empty.Count == 0 && empty.Pointer[0] == null, "empty argv");
        bool nul = false, surrogate = false, missing = false;
        try { using var bad = new ProgramArguments(["a\0b"]); } catch (ArgumentException) { nul = true; }
        try { using var bad = new ProgramArguments(["\ud800"]); } catch (EncoderFallbackException) { surrogate = true; }
        try { using var bad = new ProgramArguments([null!]); } catch (ArgumentNullException) { missing = true; }
        Check(nul && surrogate && missing, "unrepresentable managed arguments rejected");
    }
}

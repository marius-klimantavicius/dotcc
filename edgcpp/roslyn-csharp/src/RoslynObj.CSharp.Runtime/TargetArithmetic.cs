namespace RoslynObj.CSharp.Runtime;

/// <summary>Small target operations that have a different C# language contract.</summary>
public static class TargetArithmetic
{
    public static int ShiftLeft(int value, int count) { CheckShift(count, 32); return unchecked(value << count); }
    public static uint ShiftLeft(uint value, int count) { CheckShift(count, 32); return value << count; }
    public static long ShiftLeft(long value, int count) { CheckShift(count, 64); return unchecked(value << count); }
    public static ulong ShiftLeft(ulong value, int count) { CheckShift(count, 64); return value << count; }
    public static int ShiftRight(int value, int count) { CheckShift(count, 32); return value >> count; }
    public static uint ShiftRight(uint value, int count) { CheckShift(count, 32); return value >> count; }
    public static long ShiftRight(long value, int count) { CheckShift(count, 64); return value >> count; }
    public static ulong ShiftRight(ulong value, int count) { CheckShift(count, 64); return value >> count; }

    private static void CheckShift(int count, int bits)
    {
        if ((uint)count >= bits) throw new InvalidOperationException("The translated program executed a shift outside its target operand width.");
    }
}

public static class RuntimeContract
{
    public static void RequireLayout(int pointerBytes, bool littleEndian)
    {
        if (IntPtr.Size != pointerBytes || BitConverter.IsLittleEndian != littleEndian)
            throw new PlatformNotSupportedException("Generated storage requires the captured pointer size and endianness.");
    }
}

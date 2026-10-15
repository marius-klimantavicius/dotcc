namespace RoslynObj.CSharp.Runtime;

public static partial class TargetArithmetic
{
    public static sbyte AddAssign(ref sbyte storage, int right) => storage = unchecked((sbyte)(storage + right));
    public static sbyte SubtractAssign(ref sbyte storage, int right) => storage = unchecked((sbyte)(storage - right));
    public static sbyte MultiplyAssign(ref sbyte storage, int right) => storage = unchecked((sbyte)(storage * right));
    public static sbyte DivideAssign(ref sbyte storage, int right) => storage = unchecked((sbyte)(storage / right));
    public static sbyte RemainderAssign(ref sbyte storage, int right) => storage = unchecked((sbyte)(storage % right));
    public static sbyte AndAssign(ref sbyte storage, int right) => storage = unchecked((sbyte)(storage & right));
    public static sbyte OrAssign(ref sbyte storage, int right) => storage = unchecked((sbyte)((byte)storage | right));
    public static sbyte XorAssign(ref sbyte storage, int right) => storage = unchecked((sbyte)(storage ^ right));
    public static byte AddAssign(ref byte storage, int right) => storage = unchecked((byte)(storage + right));
    public static byte SubtractAssign(ref byte storage, int right) => storage = unchecked((byte)(storage - right));
    public static byte MultiplyAssign(ref byte storage, int right) => storage = unchecked((byte)(storage * right));
    public static byte DivideAssign(ref byte storage, int right) => storage = unchecked((byte)(storage / right));
    public static byte RemainderAssign(ref byte storage, int right) => storage = unchecked((byte)(storage % right));
    public static byte AndAssign(ref byte storage, int right) => storage = unchecked((byte)(storage & right));
    public static byte OrAssign(ref byte storage, int right) => storage = unchecked((byte)(storage | right));
    public static byte XorAssign(ref byte storage, int right) => storage = unchecked((byte)(storage ^ right));
    public static short AddAssign(ref short storage, int right) => storage = unchecked((short)(storage + right));
    public static short SubtractAssign(ref short storage, int right) => storage = unchecked((short)(storage - right));
    public static short MultiplyAssign(ref short storage, int right) => storage = unchecked((short)(storage * right));
    public static short DivideAssign(ref short storage, int right) => storage = unchecked((short)(storage / right));
    public static short RemainderAssign(ref short storage, int right) => storage = unchecked((short)(storage % right));
    public static short AndAssign(ref short storage, int right) => storage = unchecked((short)(storage & right));
    public static short OrAssign(ref short storage, int right) => storage = unchecked((short)((ushort)storage | right));
    public static short XorAssign(ref short storage, int right) => storage = unchecked((short)(storage ^ right));
    public static ushort AddAssign(ref ushort storage, int right) => storage = unchecked((ushort)(storage + right));
    public static ushort SubtractAssign(ref ushort storage, int right) => storage = unchecked((ushort)(storage - right));
    public static ushort MultiplyAssign(ref ushort storage, int right) => storage = unchecked((ushort)(storage * right));
    public static ushort DivideAssign(ref ushort storage, int right) => storage = unchecked((ushort)(storage / right));
    public static ushort RemainderAssign(ref ushort storage, int right) => storage = unchecked((ushort)(storage % right));
    public static ushort AndAssign(ref ushort storage, int right) => storage = unchecked((ushort)(storage & right));
    public static ushort OrAssign(ref ushort storage, int right) => storage = unchecked((ushort)(storage | right));
    public static ushort XorAssign(ref ushort storage, int right) => storage = unchecked((ushort)(storage ^ right));
}

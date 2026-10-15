using System.Runtime.InteropServices;

namespace RoslynObj.CSharp.Runtime;

/// <summary>The pinned SysV AMD64 va_list storage, validated against captured builtin layout.</summary>
[StructLayout(LayoutKind.Explicit, Size = 24, Pack = 8)]
public unsafe struct SysVVaList
{
    [FieldOffset(0)] public uint GpOffset;
    [FieldOffset(4)] public uint FpOffset;
    [FieldOffset(8)] public byte* OverflowArgumentArea;
    [FieldOffset(16)] public byte* RegisterSaveArea;
}

/// <summary>Primitive SysV AMD64 variadic cursor operations. The enclosing call owns storage.</summary>
public static unsafe class SysVVariadic
{
    public const int GpRegisterBytes = 48;
    public const int RegisterSaveBytes = 176;

    static SysVVariadic()
    {
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
            throw new PlatformNotSupportedException("This variadic adapter requires the captured Linux SysV AMD64 ABI.");
    }

    public static byte* Align16(byte* storage) => (byte*)(((nuint)storage + 15) & ~(nuint)15);

    public static void Start(void* destination, SysVVaList* arguments)
    {
        if (arguments == null) throw new InvalidOperationException("A translated variadic call has no argument pack.");
        *(SysVVaList*)destination = *arguments;
    }

    public static void Copy(void* destination, void* source) => *(SysVVaList*)destination = *(SysVVaList*)source;

    public static void End(void* cursor)
    {
        // SysV va_end does not own/free the caller's save or overflow areas.
        // Owned managed-call packs and borrowed native frames live through the call.
        _ = cursor;
    }

    public static int NextInt32(void* cursor) => *(int*)TakeGeneral((SysVVaList*)cursor);
    public static uint NextUInt32(void* cursor) => *(uint*)TakeGeneral((SysVVaList*)cursor);
    public static long NextInt64(void* cursor) => *(long*)TakeGeneral((SysVVaList*)cursor);
    public static ulong NextUInt64(void* cursor) => *(ulong*)TakeGeneral((SysVVaList*)cursor);
    public static void* NextPointer(void* cursor) => *(void**)TakeGeneral((SysVVaList*)cursor);

    public static double NextDouble(void* cursor)
    {
        var list = (SysVVaList*)cursor;
        if (list->FpOffset <= RegisterSaveBytes - 16)
        {
            var value = *(double*)(list->RegisterSaveArea + list->FpOffset);
            list->FpOffset += 16;
            return value;
        }
        return *(double*)TakeOverflow(list);
    }

    private static byte* TakeGeneral(SysVVaList* list)
    {
        if (list->GpOffset <= GpRegisterBytes - 8)
        {
            var argument = list->RegisterSaveArea + list->GpOffset;
            list->GpOffset += 8;
            return argument;
        }
        return TakeOverflow(list);
    }

    private static byte* TakeOverflow(SysVVaList* list)
    {
        var argument = (byte*)(((nuint)list->OverflowArgumentArea + 7) & ~(nuint)7);
        list->OverflowArgumentArea = argument + 8;
        return argument;
    }
}

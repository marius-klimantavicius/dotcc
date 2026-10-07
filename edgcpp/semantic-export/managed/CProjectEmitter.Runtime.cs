using System.Text;

namespace Edg.Semantic;

public sealed partial class CProjectEmitter
{
    private string RuntimeSource()
    {
        var missing = externalRoutines.Select(id => graph.Get(id)!).Where(r =>
            r.Name is not ("sqlite3_os_init" or "sqlite3_os_end") && RuntimeBinding(r.Name ?? "", "void") == null).Select(r => r.Name).Distinct().ToArray();
        if (missing.Length != 0) throw new InvalidDataException("Missing managed runtime bindings: " + string.Join(", ", missing));
        var text = new StringBuilder(Header);
        text.AppendLine("""
    private static readonly global::System.Collections.Generic.List<byte[]> __globalRoots = new();
    private static void* __AllocateGlobal(ulong size, ulong alignment)
    {
        if (size > int.MaxValue || alignment == 0 || (alignment & (alignment - 1)) != 0)
            throw new InvalidOperationException("Invalid exported global layout");
        byte[] bytes = GC.AllocateArray<byte>(checked((int)(size + alignment)), pinned: true);
        __globalRoots.Add(bytes);
        byte* first = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(bytes));
        return (void*)(((nuint)first + (nuint)alignment - 1) & ~((nuint)alignment - 1));
    }
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct __CArg
    {
        [FieldOffset(0)] public ulong Low;
        [FieldOffset(8)] public ulong High;
        public static __CArg From<T>(T value) where T : unmanaged
        {
            if (sizeof(T) > sizeof(__CArg)) throw new NotSupportedException("Large vararg value");
            __CArg result = default; Unsafe.WriteUnaligned(&result, value); return result;
        }
        public static __CArg Pointer(void* value) => From((ulong)value);
    }
    [StructLayout(LayoutKind.Sequential, Size = 24)]
    public struct __VaList { public __CArg* Data; public int Count; public int Index; }
    private static int __VaStart(__VaList* list, __CArg* data, int count) { list->Data = data; list->Count = count; list->Index = 0; return 0; }
    private static int __VaCopy(__VaList* target, __VaList* source) { *target = *source; return 0; }
    private static int __VaEnd(__VaList* list) { *list = default; return 0; }
    private static T __VaTake<T>(__VaList* list) where T : unmanaged
    {
        if ((uint)list->Index >= (uint)list->Count || sizeof(T) > sizeof(__CArg))
            throw new InvalidOperationException("Invalid translated va_arg access");
        return Unsafe.ReadUnaligned<T>(list->Data + list->Index++);
    }
    private static void* __Malloc(ulong count)
    {
        try { return NativeMemory.Alloc(checked((nuint)count)); }
        catch (OutOfMemoryException) { return null; }
    }
    private static void* __Realloc(void* value, ulong count)
    {
        try { return NativeMemory.Realloc(value, checked((nuint)count)); }
        catch (OutOfMemoryException) { return null; }
    }
    private static void* __Copy(void* target, void* source, ulong count)
    {
        // Span.CopyTo has memmove overlap semantics and chunks large requests.
        if (target == source || count == 0) return target;
        if ((byte*)target > (byte*)source && (ulong)((byte*)target - (byte*)source) < count)
            for (ulong i = count; i != 0; --i) ((byte*)target)[i - 1] = ((byte*)source)[i - 1];
        else
        {
            ulong offset = 0;
            while (offset < count) { int length = (int)Math.Min((ulong)int.MaxValue, count - offset); new ReadOnlySpan<byte>((byte*)source + offset, length).CopyTo(new Span<byte>((byte*)target + offset, length)); offset += (ulong)length; }
        }
        return target;
    }
    private static void* __Set(void* target, int value, ulong count)
    {
        ulong offset = 0;
        while (offset < count) { int length = (int)Math.Min((ulong)int.MaxValue, count - offset); new Span<byte>((byte*)target + offset, length).Fill((byte)value); offset += (ulong)length; }
        return target;
    }
    private static ulong __Length(sbyte* value) { ulong length = 0; while (value[length] != 0) length++; return length; }
    private static int __Compare(void* left, void* right, ulong count)
    { for (ulong i = 0; i < count; ++i) { int result = ((byte*)left)[i] - ((byte*)right)[i]; if (result != 0) return result; } return 0; }
    private static int __StringCompare(sbyte* left, sbyte* right, ulong count)
    { for (ulong i = 0; i < count; ++i) { int result = ((byte*)left)[i] - ((byte*)right)[i]; if (result != 0 || left[i] == 0) return result; } return 0; }
    private static void* __Find(void* text, int value, ulong count)
    { for (ulong i = 0; i < count; ++i) if (((byte*)text)[i] == (byte)value) return (byte*)text + i; return null; }
    private static sbyte* __StringFind(sbyte* text, int value, bool last)
    { sbyte* found = null; for (sbyte* p = text; ; ++p) { if ((byte)*p == (byte)value) { found = p; if (!last) return p; } if (*p == 0) return found; } }
    private static ulong __Span(sbyte* text, sbyte* set, bool complement)
    { ulong i = 0; while (text[i] != 0) { bool found = __StringFind(set, text[i], false) != null; if (found == complement) break; ++i; } return i; }
    [StructLayout(LayoutKind.Sequential)]
    private struct __LocalTm
    {
        public int Second, Minute, Hour, Day, Month, Year, Weekday, Yearday, IsDst;
        public long Offset;
        public sbyte* Zone;
    }
    private static __LocalTm* __localTm;
    private static __LocalTm* __LocalTime(long* epoch)
    {
        if (epoch == null) return null;
        try
        {
            DateTimeOffset local = DateTimeOffset.FromUnixTimeSeconds(*epoch).ToLocalTime();
            if (__localTm == null) __localTm = (__LocalTm*)__AllocateGlobal((ulong)sizeof(__LocalTm), 8);
            *__localTm = new __LocalTm { Second = local.Second, Minute = local.Minute, Hour = local.Hour,
                Day = local.Day, Month = local.Month - 1, Year = local.Year - 1900, Weekday = (int)local.DayOfWeek,
                Yearday = local.DayOfYear - 1, IsDst = TimeZoneInfo.Local.IsDaylightSavingTime(local) ? 1 : 0,
                Offset = (long)local.Offset.TotalSeconds, Zone = null };
            return __localTm;
        }
        catch (ArgumentOutOfRangeException) { return null; }
    }
""");
        foreach (var id in externalRoutines.Order())
        {
            var routine = graph.Get(id)!;
            var type = Canonical(Need(routine, "type")); var signature = Need(type, "signature");
            var ret = TypeName(Need(type, "returnType"));
            var parameters = graph.Linked(signature.ReferenceId("parameters")).Select(n => TypeName(Need(n, "type"))).ToArray();
            var args = parameters.Select((p, i) => p + " p" + i).ToList();
            if (signature.Flag("variadic")) args.Add("ReadOnlySpan<__CArg> __args");
            var name = routine.Name ?? "";
            if (name is "sqlite3_os_init" or "sqlite3_os_end")
            { text.AppendLine($"    public static partial {ret} {Name(routine)}({string.Join(", ", args)});"); continue; }
            var code = RuntimeBinding(name, ret);
            if (code == null) Fail(routine, "No managed runtime binding for external routine");
            text.AppendLine($"    public static {ret} {Name(routine)}({string.Join(", ", args)}) {{ unchecked {{ {code} }} }}");
        }
        text.Append(Footer); return text.ToString();
    }

    private static string? RuntimeBinding(string name, string result)
    {
        var math = name switch
        {
            "acos" => "Acos", "acosh" => "Acosh", "asin" => "Asin", "asinh" => "Asinh", "atan" => "Atan", "atanh" => "Atanh",
            "ceil" => "Ceiling", "cos" => "Cos", "cosh" => "Cosh", "exp" => "Exp", "fabs" => "Abs", "floor" => "Floor",
            "log" => "Log", "log10" => "Log10", "log2" => "Log2", "sin" => "Sin", "sinh" => "Sinh", "sqrt" => "Sqrt",
            "tan" => "Tan", "tanh" => "Tanh", "trunc" => "Truncate", _ => null
        };
        if (math != null) return $"return ({result})Math.{math}(p0);";
        return name switch
        {
            "malloc" => $"return ({result})__Malloc(p0);",
            "calloc" => $"if(p1 != 0 && p0 > ulong.MaxValue / p1) return null; var p=__Malloc(p0*p1); if(p != null) __Set(p,0,p0*p1); return ({result})p;",
            "realloc" => $"return ({result})__Realloc(p0,p1);",
            "free" => "NativeMemory.Free(p0);",
            "memcpy" or "memmove" => $"return ({result})__Copy(p0,p1,p2);",
            "memset" => $"return ({result})__Set(p0,p1,p2);",
            "memcmp" => "return __Compare(p0,p1,p2);",
            "memchr" => $"return ({result})__Find(p0,p1,p2);",
            "strlen" => $"return ({result})__Length(p0);",
            "strcmp" => "return __StringCompare(p0,p1,ulong.MaxValue);",
            "strncmp" => "return __StringCompare(p0,p1,p2);",
            "strchr" => $"return ({result})__StringFind(p0,p1,false);",
            "strrchr" => $"return ({result})__StringFind(p0,p1,true);",
            "strspn" => $"return ({result})__Span(p0,p1,false);",
            "strcspn" => $"return ({result})__Span(p0,p1,true);",
            "strcpy" => $"return ({result})__Copy(p0,p1,__Length(p1)+1);",
            "atan2" => $"return ({result})Math.Atan2(p0,p1);",
            "pow" => $"return ({result})Math.Pow(p0,p1);",
            "fmod" => "return p0 % p1;",
            "localtime" => $"return ({result})__LocalTime(p0);",
            _ => null
        };
    }
}

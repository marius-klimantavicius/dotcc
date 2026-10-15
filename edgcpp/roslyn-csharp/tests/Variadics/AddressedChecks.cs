using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RoslynObj.CSharp.Runtime;
using Translated;
using Application = Translated.Program;

// The entry is the generated canonical C ABI wrapper, never the reference
// source body. Native helpers only echo/invoke the supplied pointer.
internal static unsafe class AddressedVariadicChecks
{
    [DllImport("variadic-adapter", EntryPoint = "native_echo_variadic")] private static extern nint Echo(nint callback);
    [DllImport("variadic-adapter", EntryPoint = "native_call_integer_variadic")] private static extern int Integers(nint callback);
    [DllImport("variadic-adapter", EntryPoint = "native_call_double_variadic")] private static extern int Doubles(nint callback);

    [StructLayout(LayoutKind.Sequential)]
    private struct GeneratedAlignmentProbe { public byte Lead; public __va_list_tag Value; }
    [StructLayout(LayoutKind.Sequential)]
    private struct RuntimeAlignmentProbe { public byte Lead; public SysVVaList Value; }

    public static int Main()
    {
        var result = Application.fixture();
        var entry = Application.table->invoke;
        var echoed = Echo(entry);
        var integers = Integers(echoed);
        var doubles = Doubles(echoed);
        if (result != 216 || integers != 55 || doubles != 120 || entry == 0 || entry != echoed)
            throw new InvalidOperationException("Borrowed native variadic cursor or canonical entry identity changed.");
        Console.WriteLine($"result={result}");
        Console.WriteLine($"entry-integer={integers}");
        Console.WriteLine($"entry-double={doubles}");
        Console.WriteLine($"entry-identity={(entry == echoed ? 1 : 0)}");

        Require(Unsafe.SizeOf<__va_list_tag>() == 24 && Unsafe.SizeOf<SysVVaList>() == 24, "physical cursor sizes");
        Require((int)Marshal.OffsetOf<__va_list_tag>(nameof(__va_list_tag.gp_offset)) == 0 &&
                (int)Marshal.OffsetOf<__va_list_tag>(nameof(__va_list_tag.fp_offset)) == 4 &&
                (int)Marshal.OffsetOf<__va_list_tag>(nameof(__va_list_tag.overflow_arg_area)) == 8 &&
                (int)Marshal.OffsetOf<__va_list_tag>(nameof(__va_list_tag.reg_save_area)) == 16, "generated cursor offsets");
        Require((int)Marshal.OffsetOf<SysVVaList>(nameof(SysVVaList.GpOffset)) == 0 &&
                (int)Marshal.OffsetOf<SysVVaList>(nameof(SysVVaList.FpOffset)) == 4 &&
                (int)Marshal.OffsetOf<SysVVaList>(nameof(SysVVaList.OverflowArgumentArea)) == 8 &&
                (int)Marshal.OffsetOf<SysVVaList>(nameof(SysVVaList.RegisterSaveArea)) == 16, "runtime cursor offsets");
        Require((int)Marshal.OffsetOf<GeneratedAlignmentProbe>(nameof(GeneratedAlignmentProbe.Value)) == 8 &&
                (int)Marshal.OffsetOf<RuntimeAlignmentProbe>(nameof(RuntimeAlignmentProbe.Value)) == 8, "physical cursor alignment");
        GeneratedAlignmentProbe generated = new() { Lead = 1, Value = default };
        RuntimeAlignmentProbe runtime = new() { Lead = 2, Value = default };
        Require((nuint)(&generated.Value) % 8 == 0 && (nuint)(&runtime.Value) % 8 == 0 &&
                (byte*)(&generated.Value) - (byte*)(&generated) == 8 && (byte*)(&runtime.Value) - (byte*)(&runtime) == 8 &&
                generated.Lead == 1 && runtime.Lead == 2, "actual cursor addresses and padding");
        Console.WriteLine("physical-cursor=passed");
        return 0;
    }

    private static void Require(bool condition, string check)
    {
        if (!condition) throw new InvalidOperationException("Variadic ABI physical check failed: " + check);
    }
}

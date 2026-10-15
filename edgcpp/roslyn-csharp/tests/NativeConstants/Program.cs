using System.Runtime.CompilerServices;
using Application = Translated.Program;

internal static unsafe class NativeConstantChecks
{
    private static void Require(bool condition, string check)
    {
        if (!condition) throw new InvalidOperationException("Generated native constant contract failed: " + check);
    }

    private static int Main()
    {
        Require(Unsafe.SizeOf<nint>() == 8 && Unsafe.SizeOf<nuint>() == 8, "captured native integer width");
        Require(unchecked((ulong)(nint)(-1)) == ulong.MaxValue, "host native pointer all-ones representation");
        var dataAddress = *Application.stored_function_address;
        var routineAddress = *Application.stored_routine_address;
        Require(dataAddress != null && routineAddress != 0, "materialized function addresses");
        Require(unchecked((nint)dataAddress) == routineAddress, "captured data-pointer view keeps canonical function identity");

        Require(Application.fixture() == 57, "fixture result");
        var observed = (int*)Application.observations;
        int[] expected = [1, 1, 1, 1, 42];
        for (var index = 0; index < expected.Length; ++index)
        {
            Require(observed[index] == expected[index], "application observation " + index);
            Console.WriteLine($"observation[{index}]={observed[index]}");
        }
        Require(observed[0] == 1 && observed[1] == 1, "application native-width unsigned max and routine-pointer sentinel comparisons");

        var value = 42;
        ((delegate* unmanaged[Cdecl]<void*, void>)dataAddress)(&value);
        ((delegate* unmanaged[Cdecl]<void*, void>)routineAddress)(&value);
        Require(value == 44, "both stored pointer views invoke the generated ordinary body");
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        Require(*Application.stored_function_address == dataAddress && *Application.stored_routine_address == routineAddress,
            "static canonical identity after GC");
        Console.WriteLine("result=57");
        Console.WriteLine("native-constants=passed");
        return 0;
    }
}

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Translated;
using Application = Translated.Program;

// Physical generated-storage acceptance. This is separate from EDG-folded
// sizeof/alignof constants and from the independently built native application.
internal static unsafe class PhysicalStorageChecks
{
    [DllImport("storage-layout-adapter", EntryPoint = "native_bits_signed")] private static extern int NativeBitsSigned(void* storage);
    [DllImport("storage-layout-adapter", EntryPoint = "native_bits_payload")] private static extern uint NativeBitsPayload(void* storage);
    [DllImport("storage-layout-adapter", EntryPoint = "native_bits_tail")] private static extern uint NativeBitsTail(void* storage);
    [DllImport("storage-layout-adapter", EntryPoint = "native_packed_value")] private static extern uint NativePackedValue(void* storage);
    [DllImport("storage-layout-adapter", EntryPoint = "native_row_left")] private static extern int NativeRowLeft(void* storage, int index);
    [DllImport("storage-layout-adapter", EntryPoint = "native_row_right")] private static extern int NativeRowRight(void* storage, int index);
    [DllImport("storage-layout-adapter", EntryPoint = "native_node_cycle")] private static extern int NativeNodeCycle(void* storage);
    [DllImport("storage-layout-adapter", EntryPoint = "native_node_element")] private static extern int NativeNodeElement(void* storage);

    public static int Main()
    {
        var result = Application.fixture();
        Require(result == 147423, "semantic observations");
        Console.WriteLine($"result={result}");
        for (var selector = 0; selector < 32; ++selector)
            Console.WriteLine($"{selector}:{Application.storage_observe(selector)}");
        Require(Unsafe.SizeOf<Bits>() == 8 && Unsafe.SizeOf<Small>() == 4 && Unsafe.SizeOf<Packed>() == 5 && Unsafe.SizeOf<Node>() == 24, "physical CLR sizes");
        Require((int)Marshal.OffsetOf<Small>(nameof(Small.right)) == 2, "Small.right offset");
        Require((int)Marshal.OffsetOf<Packed>(nameof(Packed.value)) == 1, "Packed.value offset");
        Require((int)Marshal.OffsetOf<Node>(nameof(Node.next)) == 8 && (int)Marshal.OffsetOf<Node>(nameof(Node.element)) == 16, "Node pointer offsets");
        Require((nuint)Application.bits % 4 == 0 && (nuint)Application.first % 8 == 0 && (nuint)Application.rows % 2 == 0, "actual global address alignment");
        Require(NativeBitsSigned(Application.bits) == -14 && NativeBitsPayload(Application.bits) == 4961 && NativeBitsTail(Application.bits) == 19, "native reads emitted bitfield bytes");
        Require(NativePackedValue(Application.packed) == 0x10203040u, "native reads packed unaligned field");
        Require(NativeRowLeft(Application.rows, 0) == -2 && NativeRowLeft(Application.rows, 1) == 7 && NativeRowRight(Application.rows, 1) == 9, "native reads record array stride and members");
        Require(NativeNodeCycle(Application.first) == 1 && NativeNodeElement(Application.first) == 8 && NativeNodeElement(Application.second) == 21, "native observes cyclic pointer fixups");
        var embedded = (byte*)Application.text;
        Require(embedded[0] == 'r' && embedded[1] == 'o' && embedded[2] == 0 && embedded[3] == 'x' && embedded[4] == 0, "physical exact string units and terminator");
        var firstAddress = (nuint)Application.first;
        var rowsAddress = (nuint)Application.rows;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        Require((nuint)Application.first == firstAddress && (nuint)Application.rows == rowsAddress && NativeNodeCycle(Application.first) == 1, "pinned owner address stability");
        Console.WriteLine("physical-layout=passed");
        return 0;
    }

    private static void Require(bool condition, string check)
    {
        if (!condition) throw new InvalidOperationException("Physical storage check failed: " + check);
    }
}

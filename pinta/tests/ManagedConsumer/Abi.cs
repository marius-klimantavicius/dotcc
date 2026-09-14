using System.Runtime.InteropServices;
using VM = Managed.Interpreters.Pinta;

internal static unsafe class Abi
{
    public static void Verify()
    {
        Size<VM.PintaHeapObject>(24);
        Size<VM.PintaHeapObjectData>(16);
        Size<VM.PintaReference>(8);
        Size<VM.PintaNativeFrame>(24);
        Size<VM.PintaStackFrame>(88);
        Size<VM.PintaModule>(84);
        Size<VM.PintaModuleFunction>(20);
        Size<VM.PintaApi>(72);
        Size<VM.PintaApiEnvironment>(72);
        Size<VM.PintaApiString>(16);
        Size<VM.PintaCore>(2432);
        Size<VM.PintaThread>(80);
        Size<VM.PintaHeapCache>(5616);
        Size<VM.PintaPropertyTable>(8);
        Size<VM.PintaPropertyValue>(8);
        Size<VM.PintaPropertyAccessor>(16);
        Size<VM.PintaPropertyNative>(16);
        Size<VM.PintaProperty>(32);
        Size<VM.PintaPropertySlot>(8);
        Offset<VM.PintaHeapObject>(nameof(VM.PintaHeapObject.data), 8);
        Offset<VM.PintaNativeFrame>(nameof(VM.PintaNativeFrame.length), 8);
        Offset<VM.PintaNativeFrame>(nameof(VM.PintaNativeFrame.next), 16);
        Offset<VM.PintaStackFrame>(nameof(VM.PintaStackFrame.return_address), 8);
        Offset<VM.PintaStackFrame>(nameof(VM.PintaStackFrame.function_this), 80);
        Offset<VM.PintaModule>(nameof(VM.PintaModule.strings_length), 40);
        Offset<VM.PintaModule>(nameof(VM.PintaModule.data_offset), 80);
        Offset<VM.PintaApi>(nameof(VM.PintaApi.execute), 40);
        Offset<VM.PintaApiEnvironment>(nameof(VM.PintaApiEnvironment.file_open), 40);
        Offset<VM.PintaApiEnvironment>(nameof(VM.PintaApiEnvironment.platform_encoding), 32);
        Offset<VM.PintaProperty>(nameof(VM.PintaProperty.key), 8);
        Offset<VM.PintaProperty>(nameof(VM.PintaProperty.value), 16);
        Offset<VM.PintaPropertyNative>(nameof(VM.PintaPropertyNative.native_token), 8);
        Offset<VM.PintaPropertySlot>(nameof(VM.PintaPropertySlot.property_id), 4);
        VM.PintaPropertySlot slot = new()
        { is_valid = 1, is_enumerable = 1, is_writeable = 1, is_native = 1, property_id = 0x12345678 };
        if (!new ReadOnlySpan<byte>(&slot, sizeof(VM.PintaPropertySlot)).SequenceEqual(
            new byte[] { 0x55, 0, 0, 0, 0x78, 0x56, 0x34, 0x12 }))
            throw new InvalidOperationException("ABI property-slot flag storage differs.");
        Console.WriteLine("PASS ABI property-slot bytes=5500000078563412");
    }
    private static void Size<T>(int native) where T : unmanaged
    {
        if (sizeof(T) != native) throw new InvalidOperationException($"ABI sizeof {typeof(T).Name}: managed {sizeof(T)}, native {native}");
        Console.WriteLine($"PASS ABI sizeof {typeof(T).Name}={native}");
    }
    private static void Offset<T>(string field, int native) where T : unmanaged
    {
        int actual = Marshal.OffsetOf<T>(field).ToInt32();
        if (actual != native) throw new InvalidOperationException($"ABI {typeof(T).Name}.{field}: managed {actual}, native {native}");
        Console.WriteLine($"PASS ABI offset {typeof(T).Name}.{field}={native}");
    }
}

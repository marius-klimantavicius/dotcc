using System;
using System.Runtime.InteropServices;
using Managed.Libuv.Probes;
unsafe
{
    static string Text(byte* p) => Marshal.PtrToStringUTF8((nint)p)!;
    Console.WriteLine($"{DomainA.uv_version()} {Text(DomainA.uv_version_string())}");
    byte* destination = stackalloc byte[4];
    fixed (byte* inputValue = "hello\0"u8)
    {
        long result = DomainA.uv__strscpy(destination, inputValue, 4);
        Console.WriteLine($"{result} {Text(destination)}");
    }
    byte* input = stackalloc byte[] { 111, 110, 101, 58, 58, 116, 104, 114, 101, 101, 0 };
    byte* iterator = null;
    fixed (byte* separator = ":\0"u8)
    {
        Console.WriteLine(Text(DomainA.uv__strtok(input, separator, &iterator)));
        Console.WriteLine(Text(DomainA.uv__strtok(null, separator, &iterator)));
        Console.WriteLine(Text(DomainA.uv__strtok(null, separator, &iterator)));
        Console.WriteLine(DomainA.uv__strtok(null, separator, &iterator) == null ? 1 : 0);
    }
    // Two independent emitted type/runtime domains coexist in the same assembly.
    if (DomainB.uv_version() != DomainA.uv_version() || Text(DomainB.uv_version_string()) != Text(DomainA.uv_version_string())) return 1;
    if (typeof(DomainA.uv_errno_t) == typeof(DomainB.uv_errno_t)) return 2;
}
return 0;

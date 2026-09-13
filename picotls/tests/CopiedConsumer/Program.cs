using System;
using Managed.Security;
using DotCcPointers = System.Int32;

// Consumer names may coexist with nested translation types and file-local aliases.
namespace Managed.Security
{
    public static class Libc { public const int Value = 17; }
    public struct st_ptls_iovec_t { public int Value; }
}

internal static unsafe class Program
{
    private static void Main()
    {
        DotCcPointers local = Libc.Value;
        st_ptls_iovec_t consumer = new() { Value = local };
        PicoTls.st_ptls_iovec_t translated = new() { len = 23 };
        if (consumer.Value != 17 || translated.len != 23)
            throw new InvalidOperationException("Consumer type or alias collision");
        fixed (byte* address = "127.0.0.1\0"u8)
        {
            if (PicoTls.ptls_server_name_is_ipaddr(address) == 0 ||
                PicoTls.PicoTlsFunctionPointers.ptls_server_name_is_ipaddr(address) == 0)
                throw new InvalidOperationException("Copied API/callback failed");
        }
        byte* storage = (byte*)PicoTls.Libc.malloc(32);
        if (storage == null) throw new OutOfMemoryException();
        try { storage[31] = 42; if (storage[31] != 42) throw new InvalidOperationException(); }
        finally { PicoTls.Libc.free(storage); }
        Console.WriteLine("PASS copied PicoTls sources: consumer aliases/types, nested runtime and callbacks; implicit usings disabled");
    }
}

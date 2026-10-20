// Test-only Linux guard-page host. No host interop is compiled into the product.
using System;
using System.Runtime.InteropServices;
using System.Text.Json;
using VM = Managed.Zstd.PrimitiveTest.CPrimitives;

internal static unsafe class Program
{
    [DllImport("libc", SetLastError = true)]
    private static extern void* mmap(void* address, nuint length, int protection, int flags, int fd, nint offset);
    [DllImport("libc", SetLastError = true)]
    private static extern int mprotect(void* address, nuint length, int protection);
    [DllImport("libc", SetLastError = true)]
    private static extern int munmap(void* address, nuint length);

    private static int Main(string[] args)
    {
        using var owner = new VM.Libc.RuntimeContext();
        using var binding = owner.Enter();
        int line = VM.dz_primitives();
        if (line != 0) throw new InvalidOperationException("Primitive check failed at original C line " + line);
        int page = Environment.SystemPageSize;
        void* area = mmap(null, (nuint)(page * 2), 3, 0x22, -1, 0);
        if (area == (void*)(-1)) throw new InvalidOperationException("mmap: " + Marshal.GetLastPInvokeError());
        try
        {
            if (mprotect((byte*)area + page, (nuint)page, 0) != 0)
                throw new InvalidOperationException("mprotect: " + Marshal.GetLastPInvokeError());
            if (args is ["--probe-guard"])
            {
                // The deliberate inaccessible read must terminate its isolated
                // subprocess; it proves that an accidental fourth byte faults.
                Console.WriteLine(*((byte*)area + page));
                return 0;
            }
            int guard = VM.dz_primitive_guard3((byte*)area + page - 3);
            if (guard != 0) throw new InvalidOperationException("Three-byte page-boundary check " + guard);
        }
        finally { if (munmap(area, (nuint)(page * 2)) != 0) throw new InvalidOperationException("munmap failed."); }
        Console.WriteLine(JsonSerializer.Serialize(new Result(VM.dz_primitive_checks(), VM.dz_primitive_digest().ToString("x16"), true), ResultJson.Default.Result));
        return 0;
    }
    internal sealed record Result(ulong checks, string digest, bool guard3);
}
[System.Text.Json.Serialization.JsonSerializable(typeof(Program.Result))]
internal partial class ResultJson : System.Text.Json.Serialization.JsonSerializerContext;

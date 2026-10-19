using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class PointerAtomicTests
{
    [Theory]
    [InlineData("void *")]
    [InlineData("int *")]
    public void Pointer_cas_and_exchange_use_native_integer_storage(string element)
    {
        var path=Path.Combine(Path.GetTempPath(),$"dotcc-pointer-atomic-{Guid.NewGuid():N}.c");
        File.WriteAllText(path,$$"""
            #include <stdatomic.h>
            int cas(_Atomic({{element}})*p, {{element}}*e, {{element}}v) {
                return atomic_compare_exchange_strong_explicit(p,e,v,memory_order_acq_rel,memory_order_acquire);
            }
            {{element}} exchange(_Atomic({{element}})*p, {{element}}v) { return atomic_exchange(p,v); }
            """);
        try
        {
            var emitted=Compiler.EmitCSharp(new[]{path},emit:EmitMode.ManagedLib);
            Assert.Contains("GnuAtomic.CompareExchange(ref *(nint*)(p), ref *(nint*)(e)",emitted);
            Assert.Contains("Atomic.Exchange(ref *(nint*)(p)",emitted);
        }
        finally { File.Delete(path); }
    }
}

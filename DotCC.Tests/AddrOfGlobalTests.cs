#nullable enable

using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

/// <summary>
/// Unit tests for taking the address of a fixed-address global struct field. A
/// bare <c>&amp;Globals.field</c> is still CS0212, so dotcc hands the stable address
/// back via <c>Unsafe.AsPointer(ref global::DotCcProgram.Globals.field)</c>. A genuine LOCAL
/// is a fixed variable, so its <c>&amp;</c> stays the plain form. End-to-end in
/// the <c>addr-of-global/</c> fixture.
/// </summary>
[Collection("AddrOfGlobal")]
public sealed class AddrOfGlobalTests
{
    private static string Emit(string body)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-addr-{System.Guid.NewGuid():N}.c");
        File.WriteAllText(path, body);
        try { return Compiler.EmitCSharp(new[] { path }); }
        finally { File.Delete(path); }
    }

    [Fact]
    public void address_of_global_scalar_uses_unsafe_aspointer()
    {
        var emitted = Emit("""
            int g = 0;
            int* take(void) { return &g; }
            int main(void) { return *take(); }
            """);
        emitted.ShouldContain("(int*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::DotCcProgram.Globals.g)");
        emitted.ShouldNotContain("(&g)");
        emitted.Split("[FixedAddressValueType]").Length.ShouldBe(2);
        emitted.ShouldContain("[FixedAddressValueType]\n    internal static DotCcProgramGlobals Globals;");
        emitted.ShouldNotContain("[FixedAddressValueType]\n    public int g;");
    }

    [Fact]
    public void address_of_global_struct_uses_unsafe_aspointer()
    {
        var emitted = Emit("""
            typedef struct { int a; } S;
            static S s;
            S* take(void) { return &s; }
            int main(void) { return take()->a; }
            """);
        emitted.ShouldContain("(S*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::DotCcProgram.Globals.s)");
    }

    [Fact]
    public void address_of_static_local_uses_unsafe_aspointer()
    {
        var emitted = Emit("""
            int next(void) { static int seed = 1; int* p = &seed; (*p)++; return seed; }
            int main(void) { return next(); }
            """);
        emitted.ShouldContain("Unsafe.AsPointer(ref global::DotCcProgram.Globals.seed__s0)");
    }

    [Fact]
    public void address_of_a_plain_local_stays_the_bare_form()
    {
        // Guard: a real local is a fixed variable — its `&` must NOT be rewritten.
        var emitted = Emit("""
            int main(void) { int x = 5; int* p = &x; return *p; }
            """);
        emitted.ShouldContain("&x");
        emitted.ShouldNotContain("Unsafe.AsPointer(ref x)");
    }

    [Fact]
    public void old_object_global_layout_requires_regeneration()
    {
        var src = Path.Combine(Path.GetTempPath(), $"dotcc-old-globals-{System.Guid.NewGuid():N}.c");
        var obj = Path.ChangeExtension(src, ".cs");
        File.WriteAllText(src, "int g = 1; int main(void) { return g - 1; }");
        try
        {
            File.WriteAllText(obj, Compiler.EmitObject(src)
                .Replace("//!!dotcc-obj globals-layout:2\n", "//!!dotcc-obj globals-layout:1\n"));
            Should.Throw<CompileException>(() => Compiler.LinkObjects(new[] { obj }))
                .Message.ShouldContain("regenerate objects");
        }
        finally { File.Delete(src); File.Delete(obj); }
    }
}

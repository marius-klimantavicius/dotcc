#nullable enable

using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

/// <summary>
/// Unit tests for C11 `_Thread_local` (C23 `thread_local`) and Zig
/// `threadlocal var` — thread storage duration, lowered to a field of a pinned
/// per-thread globals struct (the marker rides `Symbol.IsThreadLocal`, set
/// by the spec resolution / the Zig container-var lowering). V1 constraints,
/// all loud: explicit static storage at block scope, zero/default initializer only (a .NET [ThreadStatic]
/// initializer runs on the first thread only), scalars only on the Zig side.
/// End-to-end in the `c11-thread-local/` fixture (gcc `-pthread` oracle) and
/// the `threadlocal_var` Zig oracle program.
/// </summary>
[Collection("ThreadLocal")]
public sealed class ThreadLocalTests
{
    private static string WriteTemp(string body, string ext = "c")
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-tls-{System.Guid.NewGuid():N}.{ext}");
        File.WriteAllText(path, body);
        return path;
    }

    [Fact]
    public void Gnu_thread_alias_uses_the_thread_storage_contract()
    {
        var src = WriteTemp("""
            typedef int value_t;
            extern __thread value_t *current;
            __thread value_t *current;
            int main(void) { return current != 0; }
            """);
        try
        {
            Compiler.EmitCSharp(new[] { src })
                .ShouldContain("public int* current;");
        }
        finally { File.Delete(src); }
    }

    [Fact]
    public void Thread_local_global_gets_thread_static_attribute()
    {
        var src = WriteTemp("""
            _Thread_local int tls_count;
            static _Thread_local long tls_static;
            int main(void) { tls_count = 1; return tls_count - 1 + (int)tls_static; }
            """);
        try
        {
            var emitted = Compiler.EmitCSharp(new[] { src });
            emitted.ShouldContain("[ThreadStatic]\n    private static DotCcProgramGlobalsThreadLocal[] __threadGlobals;");
            emitted.ShouldContain("public int tls_count;");
            emitted.ShouldContain("public long tls_static;");
            emitted.ShouldContain("GC.AllocateUninitializedArray<DotCcProgramGlobalsThreadLocal>(1, pinned: true)");
        }
        finally { File.Delete(src); }
    }

    [Fact]
    public void Zero_initializer_is_allowed_nonzero_is_rejected()
    {
        // Zero-init matches the zero/default value .NET gives every thread's
        // slot anyway; a non-zero initializer would only reach the FIRST thread
        // ([ThreadStatic] semantics), so it is a loud compile error.
        var ok = WriteTemp("_Thread_local int a = 0; int main(void) { return a; }");
        var bad = WriteTemp("_Thread_local int b = 7; int main(void) { return b; }");
        try
        {
            Should.NotThrow(() => Compiler.EmitCSharp(new[] { ok }));
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { bad }))
                .Message.ShouldContain("non-zero-initialized _Thread_local is not supported");
        }
        finally { File.Delete(ok); File.Delete(bad); }
    }

    [Fact]
    public void Block_scope_thread_local_without_storage_class_is_rejected()
    {
        var src = WriteTemp("int main(void) { _Thread_local int x; return x; }");
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { src }))
                .Message.ShouldContain("'_Thread_local' at block scope requires");
        }
        finally { File.Delete(src); }
    }

    [Fact]
    public void Block_tls_duration_does_not_leak_to_following_statics()
    {
        var src = WriteTemp("""
            _Thread_local extern int external;
            _Thread_local int external;
            int first(void) {
              _Thread_local static int a, b;
              static int shared;
              static int array[2];
              return ++a + ++b + ++shared + ++array[0] + external;
            }
            """);
        try
        {
            var emitted = Compiler.EmitCSharp(new[] { src }, emit: EmitMode.ManagedLib);
            emitted.ShouldContain("ThreadGlobals.a__s0");
            emitted.ShouldContain("ThreadGlobals.b__s1");
            emitted.ShouldNotContain("ThreadGlobals.shared__s2");
            emitted.ShouldNotContain("__dotcc_tls_array_array__s3");
        }
        finally { File.Delete(src); }
    }

    [Theory]
    [InlineData("static _Thread_local", "c17")]
    [InlineData("_Thread_local static", "c17")]
    [InlineData("thread_local static", "c23")]
    public void Block_static_tls_uses_unique_thread_storage(string prefix, string dialect)
    {
        var src = WriteTemp($$"""
            int first(void) { {{prefix}} int value; return ++value; }
            int second(void) { {{prefix}} char value[21]; return ++value[0]; }
            int main(void) { return first() + second(); }
            """);
        try
        {
            var emitted = Compiler.EmitCSharp(new[] { src }, dialect: CDialect.Parse(dialect));
            emitted.ShouldContain("public int value__s0;");
            emitted.ShouldContain("__dotcc_tls_array_value__s1");
            emitted.ShouldContain("ThreadGlobals.value__s0");
        }
        finally { File.Delete(src); }
    }

    [Fact]
    public void Block_static_tls_rejects_nonzero_initializers_and_preserves_scope()
    {
        var nonzero = WriteTemp("int main(void) { _Thread_local static int value = 9; return value; }");
        var scope = WriteTemp("int main(void) { int outer = 7; { static _Thread_local int outer; ++outer; } return outer; }");
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { nonzero }))
                .Message.ShouldContain("non-zero-initialized _Thread_local");
            Compiler.EmitCSharp(new[] { scope }).ShouldContain("return outer;");
        }
        finally { File.Delete(nonzero); File.Delete(scope); }
    }

    [Fact]
    public void Lowercase_thread_local_works_via_threads_h_and_c23_keyword()
    {
        // C11 <threads.h> supplies the macro (withdrawn under c23, where rule-2
        // promotion makes the lowercase spelling a first-class keyword) — the
        // identical source composes under every dialect from c11 on.
        var src = WriteTemp("""
            #include <threads.h>
            thread_local int tls_v;
            int main(void) { tls_v = 1; return tls_v - 1; }
            """);
        try
        {
            foreach (var std in new[] { "c11", "c17", "c23" })
            {
                Compiler.EmitCSharp(new[] { src }, dialect: CDialect.Parse(std))
                    .ShouldContain("public int tls_v;");
            }
        }
        finally { File.Delete(src); }
    }

    [Fact]
    public void Thread_local_gated_as_c11_under_pedantic()
    {
        var src = WriteTemp("_Thread_local int x; int main(void) { return x; }");
        try
        {
            Should.Throw<CompileException>(() =>
                Compiler.EmitCSharp(new[] { src }, dialect: CDialect.Parse("c90"), warnings: WarningFlags.Default | WarningFlags.PedanticErrors))
                .Message.ShouldContain("_Thread_local");
        }
        finally { File.Delete(src); }
    }

    // ---- the Zig twofer: `threadlocal var` ---------------------------------

    [Fact]
    public void Zig_threadlocal_var_gets_thread_static_attribute()
    {
        var src = WriteTemp("threadlocal var tl: i32 = 0;\npub fn main() u8 { tl = 42; return @intCast(tl); }\n", "zig");
        try
        {
            Compiler.EmitCSharp(new[] { src })
                .ShouldContain("public int tl;");
        }
        finally { File.Delete(src); }
    }

    [Fact]
    public void Zig_function_local_threadlocal_is_rejected()
    {
        // The shared VarDecl nonterminal lets it parse; real zig rejects it at
        // parse time ("expected statement, found 'threadlocal'") — dotcc rejects
        // at lowering with a matching constraint.
        var src = WriteTemp("pub fn main() u8 {\n    threadlocal var x: i32 = 0;\n    x = 1;\n    return @intCast(x);\n}\n", "zig");
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { src }))
                .Message.ShouldContain("'threadlocal' is only allowed on a container-level `var`");
        }
        finally { File.Delete(src); }
    }

    [Fact]
    public void Zig_nonzero_threadlocal_initializer_is_rejected()
    {
        var src = WriteTemp("threadlocal var tl: i32 = 7;\npub fn main() u8 { return @intCast(tl); }\n", "zig");
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { src }))
                .Message.ShouldContain("non-zero initializer is not supported");
        }
        finally { File.Delete(src); }
    }
}

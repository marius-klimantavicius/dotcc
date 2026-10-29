using System;
using System.IO;
using System.Linq;
using DotCC.Frontends;
using DotCC.Ir;
using Xunit;

namespace DotCC.Tests;

public sealed class DeclarationSpecifierStorageTests
{
    [Theory]
    [InlineData("_Alignas(128) static struct Slice const")]
    [InlineData("const _Alignas(128) static struct Slice")]
    [InlineData("_Alignas(64) const static _Alignas(128) struct Slice")]
    [InlineData("struct Slice const static _Alignas(128)")]
    [InlineData("static _Alignas(unsigned long) _Alignas(128) struct Slice const")]
    [InlineData("_Alignas(128) static _Alignas(64) const struct Slice")]
    public void Alignment_storage_and_qualifiers_keep_one_tentative_object(string specifiers)
    {
        WithSource($$"""
            #include <stddef.h>
            struct Slice { unsigned short const *ptr; unsigned long len; };
            {{specifiers}} table;
            struct Slice const *early(void) { return &table; }
            static _Alignas(128) struct Slice const table = {0, 7};
            _Static_assert(sizeof(struct Slice) == 16 && _Alignof(struct Slice) == 8, "base type");
            _Static_assert(offsetof(struct Slice, len) == 8, "field layout");
            """, path =>
        {
            var ir = new CFrontend().BuildIr(new FrontendRequest([path]));
            Assert.DoesNotContain(ir.Diagnostics, d => d.Severity == Severity.Error);
            var table = Assert.Single(ir.Globals);
            Assert.Equal("table", table.Sym.Name);
            Assert.Equal(Storage.Static, table.Sym.Storage);
            Assert.Equal(128, table.Sym.Alignment);
            Assert.True(table.Sym.Type.Quals.HasFlag(TypeQual.Const));
            Assert.Equal(0, table.Sym.Type.AlignmentOverride);
            Assert.NotNull(table.Init);
            var returned = Assert.IsType<Return>(Assert.Single(Assert.Single(ir.Functions).Body.Stmts));
            var address = Assert.IsType<Unary>(returned.Value);
            Assert.Equal(UnOp.AddrOf, address.Op);
            Assert.Same(table.Sym, Assert.IsType<VarRef>(address.Operand).Sym);
            var identities = Assert.IsType<AggregateIdentitySet>(AggregateIdentities.Read(Compiler.EmitObject(path)));
            Assert.DoesNotContain(identities.Linkage, root => root.Symbol == "table");
        });
    }

    [Fact]
    public void Ordered_extern_scalar_and_array_declarations_merge_without_defining_extra_storage()
    {
        WithSource("""
            _Alignas(128) extern volatile int shared;
            _Alignas(128) extern const int rows[3];
            int read(void) {
                _Alignas(128) extern int volatile shared;
                _Alignas(128) extern int const rows[3];
                return shared + rows[1];
            }
            _Alignas(128) volatile int shared = 17;
            _Alignas(128) const int rows[3] = {3, 5, 7};
            """, path =>
        {
            var ir = new CFrontend().BuildIr(new FrontendRequest([path]));
            Assert.DoesNotContain(ir.Diagnostics, d => d.Severity == Severity.Error);
            Assert.Equal(2, ir.Globals.Count);
            Assert.All(ir.Globals, global => Assert.Equal(128, global.Sym.Alignment));
            var rows = ir.Globals.Single(global => global.Sym.Name == "rows");
            Assert.Equal(3, Assert.IsType<CType.Array>(rows.Sym.Type.Unqualified).Count);
            Assert.NotEmpty(Compiler.EmitObject(path));
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Private_globals_have_per_unit_symbols_and_keep_early_address_references(bool sameLayout)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-storage-identities-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var paths = new[] { Path.Combine(directory, "first.c"), Path.Combine(directory, "second.c") };
            for (var index = 0; index < paths.Length; index++)
                File.WriteAllText(paths[index], $$"""
                    struct Slice { {{(index == 0 || sameLayout ? "int value;" : "long value; int extra;")}} };
                    _Alignas(128) static const struct Slice table;
                    const void *address{{index}}(void) { return &table; }
                    static _Alignas(128) const struct Slice table = {7};
                    extern int shared;
                    int read{{index}}(void) { return shared; }
                    """ + (index == 1 ? "int shared = 19;" : ""));
            var ir = new CFrontend().BuildIr(new FrontendRequest(paths));
            Assert.DoesNotContain(ir.Diagnostics, d => d.Severity == Severity.Error);
            var tables = ir.Globals.Where(global => global.Sym.Name == "table").ToArray();
            Assert.Equal(2, tables.Length);
            Assert.NotSame(tables[0].Sym, tables[1].Sym);
            Assert.NotEqual(tables[0].Sym.TargetName, tables[1].Sym.TargetName);
            Assert.All(tables, table =>
            {
                Assert.Contains(table.Sym, ir.InternalGlobals);
                Assert.True(table.Sym.AddressTaken);
                Assert.Equal(128, table.Sym.Alignment);
                Assert.NotNull(table.Init);
            });
            for (var index = 0; index < tables.Length; index++)
            {
                var function = ir.Functions.Single(function => function.Sym.Name == "address" + index);
                var returned = Assert.IsType<Return>(Assert.Single(function.Body.Stmts));
                var address = Assert.IsType<Unary>(returned.Value);
                Assert.Same(tables[index].Sym, Assert.IsType<VarRef>(address.Operand).Sym);
            }
            var shared = Assert.Single(ir.Globals.Where(global => global.Sym.Name == "shared"));
            Assert.DoesNotContain(shared.Sym, ir.InternalGlobals);
            foreach (var function in ir.Functions.Where(function => function.Sym.Name.StartsWith("read", StringComparison.Ordinal)))
                Assert.Same(shared.Sym, Assert.IsType<VarRef>(Assert.IsType<Return>(Assert.Single(function.Body.Stmts)).Value).Sym);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Fact]
    public void Another_units_private_object_does_not_satisfy_a_block_extern_reference()
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-storage-extern-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var paths = new[] { Path.Combine(directory, "first.c"), Path.Combine(directory, "second.c") };
            File.WriteAllText(paths[0], "static int value = 7; int *private_address(void) { return &value; }");
            File.WriteAllText(paths[1], "int read(void) { extern int value; return value; }");
            var ir = new CFrontend().BuildIr(new FrontendRequest(paths));
            var storage = Assert.Single(ir.Globals).Sym;
            var read = ir.Functions.Single(function => function.Sym.Name == "read");
            var reference = Assert.IsType<VarRef>(Assert.Single(read.Body.Stmts.OfType<Return>()).Value).Sym;
            Assert.NotSame(storage, reference);
            Assert.NotEqual(storage.TargetName, reference.TargetName);
            Assert.Contains(storage, ir.InternalGlobals);
            Assert.DoesNotContain(reference, ir.InternalGlobals);
            Assert.Equal("value", Assert.Single(ir.ExternDataReferenced));

            // External definitions still share one compatibility contract.
            File.WriteAllText(paths[0], "extern int value; int read_first(void) { return value; }");
            File.WriteAllText(paths[1], "long value = 7;");
            var error = Assert.Throws<CompileException>(() => new CFrontend().BuildIr(new FrontendRequest(paths)));
            Assert.Contains("conflicting types for global 'value'", error.Message);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Theory]
    [InlineData("_Alignas(128) static const int value = 7;", "int main(void) { value = 8; return value; }")]
    [InlineData("", "int main(void) { _Alignas(128) static int const value = 7; value = 8; return value; }")]
    public void Reordered_storage_does_not_drop_const(string globals, string body)
        => WithSource(globals + body, path => Assert.Throws<CompileException>(() => Compiler.EmitObject(path)));

    [Theory]
    [InlineData("int static helper(void) { return 42; }")]
    [InlineData("typedef int Result; inline static Result helper(void) { return 42; }")]
    public void Interspersed_function_storage_retains_internal_linkage(string source)
        => WithSource(source, path =>
        {
            var ir = new CFrontend().BuildIr(new FrontendRequest([path]));
            Assert.Equal(Storage.Static, Assert.Single(ir.Functions).Sym.Storage);
        });

    [Theory]
    [InlineData("inline static int", true)]
    [InlineData("static inline int", true)]
    [InlineData("int inline static", true)]
    [InlineData("inline extern int", false)]
    [InlineData("extern inline int", false)]
    public void Function_specifiers_retain_inline_and_linkage_in_each_order(string specifiers, bool isStatic)
        => WithSource(specifiers + " helper(void) { return 42; }", path =>
        {
            var ir = new CFrontend().BuildIr(new FrontendRequest([path]));
            Assert.DoesNotContain(ir.Diagnostics, d => d.Severity == Severity.Error);
            var function = Assert.Single(ir.Functions).Sym;
            Assert.Equal(isStatic, function.Storage == Storage.Static);
            Assert.True(function.IsInline);
        });

    [Theory]
    [InlineData("_Noreturn static void", true)]
    [InlineData("static _Noreturn void", true)]
    [InlineData("void _Noreturn static", true)]
    [InlineData("_Noreturn extern void", false)]
    [InlineData("extern _Noreturn void", false)]
    public void Function_specifiers_retain_noreturn_and_linkage_in_each_order(string specifiers, bool isStatic)
        => WithSource(specifiers + " helper(void) { for (;;) {} }", path =>
        {
            var ir = new CFrontend().BuildIr(new FrontendRequest([path]));
            Assert.DoesNotContain(ir.Diagnostics, d => d.Severity == Severity.Error);
            var function = Assert.Single(ir.Functions).Sym;
            Assert.Equal(isStatic, function.Storage == Storage.Static);
            Assert.True(function.IsNoReturn);
        });

    [Theory]
    [InlineData("_Alignas(128) _Thread_local static int")]
    [InlineData("_Alignas(128) static _Thread_local int")]
    [InlineData("int _Thread_local static _Alignas(128)")]
    public void Aligned_callback_scalars_and_arrays_keep_tls_at_file_and_block_scope(string specifiers)
        => WithSource($$"""
            _Alignas(128) _Thread_local extern int (*shared_callback)(int);
            _Alignas(128) _Thread_local extern int (*shared_callbacks[2])(int);
            _Alignas(128) _Thread_local int (*shared_callback)(int);
            _Alignas(128) _Thread_local int (*shared_callbacks[2])(int);
            {{specifiers}} (*file_callback)(int);
            {{specifiers}} (*file_callbacks[2])(int);
            int read(void) {
                {{specifiers}} (*local_callback)(int);
                {{specifiers}} (*local_callbacks[2])(int);
                return shared_callback == 0 && shared_callbacks[0] == 0
                    && file_callback == 0 && file_callbacks[0] == 0
                    && local_callback == 0 && local_callbacks[0] == 0;
            }
            """, path =>
        {
            var ir = new CFrontend().BuildIr(new FrontendRequest([path]));
            Assert.DoesNotContain(ir.Diagnostics, d => d.Severity == Severity.Error);
            Assert.Equal(6, ir.Globals.Count);
            Assert.All(ir.Globals, global =>
            {
                Assert.True(global.Sym.IsThreadLocal);
                Assert.Equal(128, global.Sym.Alignment);
                if (global.Sym.Type.Unqualified is CType.Array array)
                {
                    Assert.Equal(2, array.Count);
                    Assert.IsType<CType.Func>(array.Element.Unqualified);
                }
                else Assert.IsType<CType.Func>(global.Sym.Type.Unqualified);
            });
            // Preserve the requested TLS alignment in IR and retain the
            // existing backend's explicit unsupported boundary. This grammar
            // change does not introduce per-thread aligned allocation.
            var error = Assert.Throws<IrUnsupportedException>(() => Compiler.EmitObject(path));
            Assert.Contains("over-aligned thread-local storage", error.Message);
        });

    private static void WithSource(string source, Action<string> test)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-declaration-storage-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, source);
        try { test(path); }
        finally { File.Delete(path); }
    }
}

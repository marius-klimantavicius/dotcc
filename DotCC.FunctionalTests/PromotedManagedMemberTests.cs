using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class PromotedManagedMemberTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Qualified_properties_keep_fences_and_valid_C_names_keep_physical_access(bool objects)
    {
        WithSource("""
            typedef int (*Callback)(int);
            struct Qualified { struct {
                _Atomic int ready;
                volatile unsigned count;
                int * volatile pointer;
                Callback volatile callback;
                const int *view;
                int *const fixed_pointer;
            }; };
            struct NameCollision { struct { int NameCollision; }; };
            void initialize_collision(struct NameCollision *p) { p->NameCollision = 23; }
            int read_collision(struct NameCollision *p) { return p->NameCollision; }
            """, path =>
        {
            string generated;
            if (objects)
            {
                var obj = path + ".cs";
                File.WriteAllText(obj, Compiler.EmitObject(path));
                generated = Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib, className: "Api");
            }
            else generated = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, className: "Api");
            generated.ShouldContain("return (int)Atomic.Load(ref");
            generated.ShouldContain("Atomic.Store(ref");
            generated.ShouldContain("global::System.Threading.Volatile.Read(ref");
            generated.ShouldContain("global::System.Threading.Volatile.Write(ref");
            var consumer = """
                public static unsafe class Consumer {
                    static int Add(int x) => x + 5;
                    public static int Main() {
                        Qualified value = default;
                        value.ready = 1; value.count = 27;
                        int pointee = 17;
                        value.pointer = &pointee; value.view = &pointee; value.callback = &Add;
                        if (value.ready != 1 || value.count != 27 || *value.pointer != 17 || value.callback(4) != 9) return 1;
                        if (!typeof(Qualified).GetProperty("view")!.CanWrite || typeof(Qualified).GetProperty("fixed_pointer")!.CanWrite) return 2;
                        NameCollision collision = default;
                        Api.initialize_collision(&collision);
                        return Api.read_collision(&collision) == 23 ? 0 : 3;
                    }
                }
                """;
            FixtureRunner.CompileAndRunCapturingExit(generated + consumer, Array.Empty<string>()).exit.ShouldBe(0);
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Managed_consumer_uses_promoted_values_without_storage_changes(bool objects)
    {
        WithSource("""
            typedef int (*Callback)(int);
            struct Example {
                unsigned marker;
                union {
                    unsigned long flags;
                    struct { unsigned long enabled : 1; unsigned long mode : 3; } bits;
                };
                struct {
                    union { int number; unsigned unsigned_number; };
                    Callback callback;
                    const int fixed_value;
                    int *pointer;
                };
            };
            void initialize(struct Example *p) { p->marker = 7; p->flags = 0; p->number = 42; }
            int inspect(struct Example *p) { return p->marker == 7 && p->flags == 11 && p->number == -1 && p->callback(4) == 9 && *p->pointer == 17; }
            unsigned long extent(void) { return sizeof(struct Example); }
            """, path =>
        {
            string generated;
            if (objects)
            {
                var obj = path + ".cs";
                File.WriteAllText(obj, Compiler.EmitObject(path));
                generated = Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib, className: "Api");
            }
            else generated = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, className: "Api");
            var consumer = """
                public static unsafe class Consumer {
                    static int Add(int x) => x + 5;
                    public static int Main() {
                        Example value = default;
                        Api.initialize(&value);
                        value.bits.enabled = 1;
                        value.bits.mode = 5;
                        value.unsigned_number = uint.MaxValue;
                        value.callback = &Add;
                        int pointee = 17;
                        value.pointer = &pointee;
                        if (value.number != -1 || value.fixed_value != 0 || Api.extent() != 48 || sizeof(Example) != 48) return 1;
                        if (typeof(Example).GetProperty("fixed_value")!.CanWrite) return 2;
                        if (!typeof(Example).GetProperty("callback")!.CanWrite) return 3;
                        return Api.inspect(&value) == 1 ? 0 : 4;
                    }
                }
                """;
            FixtureRunner.CompileAndRunCapturingExit(generated + consumer, Array.Empty<string>()).exit.ShouldBe(0);
        });
    }

    [Theory]
    [InlineData("struct A { int value; union { int value; unsigned other; }; };")]
    [InlineData("struct A { union { int value; unsigned other; }; int value; };")]
    [InlineData("struct A { struct { int value; }; struct { int value; }; };")]
    public void Ambiguous_promoted_names_are_rejected(string source)
        => WithSource(source, path => Should.Throw<CompileException>(() =>
            Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib)).Message.ShouldContain("ambiguous anonymous member: value"));

    private static void WithSource(string source, Action<string> test)
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-promoted-managed-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try { var path = Path.Combine(dir, "source.c"); File.WriteAllText(path, source); test(path); }
        finally { Directory.Delete(dir, true); }
    }
}

using System;
using System.IO;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed partial class ManagedLibraryTests
{
    [Theory]
    [InlineData(false, "SqliteApi", "SqliteApi")]
    [InlineData(true, "SqliteApi", "SqliteApi")]
    [InlineData(false, "class", "@class")]
    [InlineData(true, "@class", "@class")]
    [InlineData(false, "default", "@default")]
    [InlineData(true, "null", "@null")]
    public void Custom_class_name_preserves_calls_global_initializers_and_canonical_callbacks(bool link, string name, string identifier)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-class-name-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var source = Path.Combine(directory, "api.c");
            File.WriteAllText(source, """
                #include <stdlib.h>
                typedef int (*Callback)(int);
                int add(int x) { return x + 1; }
                Callback current = add;
                Callback get(void) { return &add; }
                Callback runtime(void) { return abs; }
                int run(int x) { return current(x) + add(0); }
                const char *unchanged(void) { return "DotCcLib"; }
                """);
            string emitted;
            if (link)
            {
                var fragment = Path.ChangeExtension(source, ".cs");
                File.WriteAllText(fragment, Compiler.EmitObject(source));
                emitted = Compiler.LinkObjects(new[] { fragment }, emit: EmitMode.ManagedLib, className: name);
            }
            else emitted = Compiler.EmitCSharp(new[] { source }, emit: EmitMode.ManagedLib, className: name);
            emitted.ShouldContain("public static class " + identifier);
            emitted.ShouldNotContain("global::DotCcLib;");
            var references = RuntimeReferences();
            var library = Compile("NamedLibrary_" + Guid.NewGuid().ToString("N"), emitted, references);
            references.Add(MetadataReference.CreateFromImage(library));
            var consumer = Compile("NamedConsumer_" + Guid.NewGuid().ToString("N"), $$"""
                public static unsafe class Consumer {
                    public static int Run() {
                        if ({{identifier}}.get() != {{identifier.TrimStart('@')}}FunctionPointers.add) return -1;
                        if ({{identifier}}.runtime()(-42) != 42) return -2;
                        if (System.Runtime.InteropServices.Marshal.PtrToStringUTF8((nint){{identifier}}.unchanged()) != "DotCcLib") return -3;
                        return {{identifier}}.run(40);
                    }
                }
                """, references);
            var context = new AssemblyLoadContext("named-api-" + Guid.NewGuid().ToString("N"), isCollectible: false);
            var assembly = context.LoadFromStream(new MemoryStream(library));
            assembly.GetType(name.TrimStart('@'), true)!.IsPublic.ShouldBeTrue();
            assembly.GetType("DotCcLib").ShouldBeNull();
            context.LoadFromStream(new MemoryStream(consumer)).GetType("Consumer", true)!.GetMethod("Run")!.Invoke(null, null).ShouldBe(42);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public void Native_export_wrappers_target_the_custom_library_class()
    {
        var source = Path.Combine(Path.GetTempPath(), "dotcc-named-export-" + Guid.NewGuid().ToString("N") + ".c");
        try
        {
            File.WriteAllText(source, "int add(int x) { return x + 1; }");
            var emitted = Compiler.EmitCSharp(new[] { source }, emit: EmitMode.SharedLib, className: "NativeApi");
            emitted.ShouldContain("internal static class NativeApi");
            emitted.ShouldContain("=> NativeApi.add(x)");
            emitted.ShouldContain("public static class DotCcExports");
            Compile("NamedExports", emitted, RuntimeReferences());
        }
        finally { File.Delete(source); }
    }

    [Theory]
    [InlineData("")]
    [InlineData("@")]
    [InlineData("9Api")]
    [InlineData("My.Api")]
    [InlineData("Api {}")]
    [InlineData("DotCcGlobals")]
    [InlineData("Libc")]
    public void Invalid_class_names_fail_before_reading_input(string name)
    {
        Should.Throw<CompileException>(() => Compiler.EmitCSharp(Array.Empty<string>(), emit: EmitMode.ManagedLib, className: name))
            .Message.ShouldContain("--class-name");
        Should.Throw<CompileException>(() => Compiler.LinkObjects(Array.Empty<string>(), emit: EmitMode.ManagedLib, className: name))
            .Message.ShouldContain("--class-name");
    }

    [Theory]
    [InlineData(EmitMode.File)]
    [InlineData(EmitMode.Csproj)]
    [InlineData(EmitMode.Object)]
    public void Class_name_is_only_accepted_for_library_output(EmitMode mode)
    {
        Should.Throw<CompileException>(() => Compiler.EmitCSharp(Array.Empty<string>(), emit: mode, className: "Api"))
            .Message.ShouldContain("requires managed-library or shared-library");
        Should.Throw<CompileException>(() => Compiler.LinkObjects(Array.Empty<string>(), emit: mode, className: "Api"))
            .Message.ShouldContain("requires managed-library or shared-library");
    }

    [Theory]
    [InlineData("struct Api { int x; }; int read(struct Api *p) { return p->x; }")]
    [InlineData("int Api(void) { return 1; }")]
    public void Class_name_cannot_collide_with_translated_declarations(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-name-collision-" + Guid.NewGuid().ToString("N") + ".c");
        var fragment = Path.ChangeExtension(path, ".cs");
        try
        {
            File.WriteAllText(path, source);
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, className: "Api"))
                .Message.ShouldContain("conflicts with a translated");
            File.WriteAllText(fragment, Compiler.EmitObject(path));
            Should.Throw<CompileException>(() => Compiler.LinkObjects(new[] { fragment }, emit: EmitMode.ManagedLib, className: "Api"))
                .Message.ShouldContain("conflicts with a translated");
        }
        finally { File.Delete(path); File.Delete(fragment); }
    }
}

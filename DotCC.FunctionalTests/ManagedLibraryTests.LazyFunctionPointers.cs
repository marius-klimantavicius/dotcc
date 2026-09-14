using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed partial class ManagedLibraryTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public void Function_pointer_properties_initialize_only_the_requested_address(bool objectLink, bool nested, bool accessorCollision)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-lazy-pointers-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var source = Path.Combine(directory, "callbacks.c");
        File.WriteAllText(source, """
            int field(int x) { return x + 1; }
            int unused(int x) { return x - 1; }
            """);
        if (accessorCollision) File.AppendAllText(source, "\nint get_field(int x) { return x + 2; }\nint get_get_field(int x) { return x + 3; }\n");
        try
        {
            var options = new CSharpOutputOptions { NestTypes = nested };
            string emitted;
            if (objectLink)
            {
                var obj = Path.Combine(directory, "callbacks.o");
                File.WriteAllText(obj, Compiler.EmitObject(source));
                emitted = Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib, outputOptions: options);
            }
            else emitted = Compiler.EmitCSharp(new[] { source }, emit: EmitMode.ManagedLib, outputOptions: options);
            var references = RuntimeReferences();
            var library = Compile("LazyLibrary_" + Guid.NewGuid().ToString("N"), emitted, references);
            references.Add(MetadataReference.CreateFromImage(library));
            var owner = nested ? "DotCcLib.DotCcLibFunctionPointers" : "DotCcLibFunctionPointers";
            var consumer = Compile("LazyConsumer_" + Guid.NewGuid().ToString("N"), $$"""
                public static unsafe class Consumer
                {
                    public static int Run()
                    {
                        var callback = {{owner}}.field;
                        for (int i = 0; i < 30000; i++)
                            if ({{owner}}.field != callback || callback(41) != 42) return -1;
                        {{(accessorCollision ? $"if ({owner}.get_field(40) != 42 || {owner}.get_get_field(39) != 42) return -2;" : "")}}
                        return 42;
                    }
                }
                """, references);
            var context = new AssemblyLoadContext("dotcc-lazy-" + Guid.NewGuid(), isCollectible: false);
            var assembly = context.LoadFromStream(new MemoryStream(library));
            var pointers = assembly.GetType(nested ? "DotCcLib+DotCcLibFunctionPointers" : owner, true)!;
            // No class initializer captures all addresses eagerly. Each property
            // owns private compiler-generated storage that remains zero until read.
            pointers.TypeInitializer.ShouldBeNull();
            pointers.GetFields().ShouldBeEmpty();
            var requested = pointers.GetProperty("field", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)!;
            requested.CanRead.ShouldBeTrue();
            requested.CanWrite.ShouldBeFalse();
            requested.PropertyType.IsFunctionPointer.ShouldBeTrue();
            var cache = requested.DeclaringType!.GetField("<field>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static)!;
            var unused = pointers.GetProperty("unused", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)!
                .DeclaringType!.GetField("<unused>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static)!;
            cache.DeclaringType!.TypeInitializer.ShouldBeNull();
            cache.GetValue(null).ShouldBe((nint)0);
            unused.GetValue(null).ShouldBe((nint)0);
            var caller = context.LoadFromStream(new MemoryStream(consumer));
            caller.GetType("Consumer", true)!.GetMethod("Run")!.Invoke(null, null).ShouldBe(42);
            cache.GetValue(null).ShouldNotBe((nint)0);
            unused.GetValue(null).ShouldBe((nint)0);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}

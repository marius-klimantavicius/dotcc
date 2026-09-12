using System;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed partial class ManagedLibraryTests
{
    [Fact]
    public void Separate_objects_preserve_variadic_callbacks_and_cursor_forwarding()
    {
        var fixture = Path.Combine(AppContext.BaseDirectory, "Fixtures", "variadic-function-pointer");
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-vararg-objects-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var objects = Directory.GetFiles(fixture, "*.c").OrderBy(path => path, StringComparer.Ordinal)
                .Select(path =>
                {
                    var fragment = Path.Combine(directory, Path.GetFileNameWithoutExtension(path) + ".cs");
                    File.WriteAllText(fragment, Compiler.EmitObject(path));
                    return fragment;
                }).ToArray();
            FixtureRunner.CompileAndRun(Compiler.LinkObjects(objects), Array.Empty<string>())
                .ShouldBe("42 42 1 1\n");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Managed_consumer_uses_cached_variadic_pointer_and_explicit_span_callback(bool objectLink)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-span-callback-" + Guid.NewGuid().ToString("N") + ".c");
        var fragment = Path.ChangeExtension(path, ".cs");
        File.WriteAllText(path, """
            #include <stdarg.h>
            typedef int (*Callback)(int, ...);
            int total(int count, ...) {
                va_list ap;
                va_start(ap, count);
                int result = 0;
                for (int i = 0; i < count; i++) result += va_arg(ap, int);
                va_end(ap);
                return result;
            }
            int relay(Callback callback) { return callback(2, 20, 22); }
            """);
        try
        {
            string emitted;
            if (objectLink)
            {
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                emitted = Compiler.LinkObjects(new[] { fragment }, emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib);
            emitted.ShouldContain("delegate*<int, System.ReadOnlySpan<VaArg>, int>");
            var references = RuntimeReferences();
            var library = Compile("SpanCallbackLibrary_" + Guid.NewGuid().ToString("N"), emitted, references);
            references.Add(MetadataReference.CreateFromImage(library));
            var consumer = Compile("SpanCallbackConsumer_" + Guid.NewGuid().ToString("N"), """
                public static unsafe class Consumer
                {
                    private static readonly delegate*<int, System.ReadOnlySpan<Libc.VaArg>, int> Callback = &Read;
                    private static int Read(int count, System.ReadOnlySpan<Libc.VaArg> args)
                    {
                        int total = 0;
                        for (int i = 0; i < count; i++) total += (int)args[i];
                        return total;
                    }
                    public static int Run()
                    {
                        var pointer = DotCcLibFunctionPointers.total;
                        if (pointer(0, []) != 0 || pointer(2, [20, 22]) != 42) return -1;
                        System.GC.Collect();
                        return DotCcLib.relay(Callback);
                    }
                }
                """, references);
            var context = new AssemblyLoadContext("dotcc-span-callback-" + Guid.NewGuid().ToString("N"), isCollectible: false);
            context.LoadFromStream(new MemoryStream(library));
            context.LoadFromStream(new MemoryStream(consumer)).GetType("Consumer", true)!
                .GetMethod("Run")!.Invoke(null, null).ShouldBe(42);
        }
        finally { File.Delete(path); File.Delete(fragment); }
    }
}

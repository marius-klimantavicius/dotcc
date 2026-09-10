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
    [InlineData(false)]
    [InlineData(true)]
    public void Canonical_function_addresses_survive_tables_cross_units_and_managed_consumers(bool objectLink)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-identity-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var first = Path.Combine(directory, "first.c");
        var second = Path.Combine(directory, "second.c");
        File.WriteAllText(first, """
            #include <stdlib.h>
            typedef int (*Callback)(int);
            int add(int x) { return x + 1; }
            Callback table[] = {add, &add, 0};
            Callback get_first(void) { return add; }
            Callback get_runtime(void) { return abs; }
            int accepts(Callback p) { return p == add && p == &add && p == table[0] && p == table[1]; }
            int null_and_sentinel(void) { return table[2] == 0 && (void*)(Callback)-1 == (void*)-1; }
            """);
        File.WriteAllText(second, """
            #include <stdlib.h>
            typedef int (*OtherName)(int);
            int add(int);
            OtherName elsewhere = &add;
            OtherName get_second(void) { return add; }
            OtherName get_runtime_second(void) { return &abs; }
            int other_accepts(OtherName p) { return p == elsewhere && p == &add; }
            """);
        try
        {
            string emitted;
            if (objectLink)
            {
                var a = Path.ChangeExtension(first, ".cs");
                var b = Path.ChangeExtension(second, ".cs");
                File.WriteAllText(a, Compiler.EmitObject(first));
                File.WriteAllText(b, Compiler.EmitObject(second));
                emitted = Compiler.LinkObjects(new[] { b, a }, emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp(new[] { second, first }, emit: EmitMode.ManagedLib);
            var references = RuntimeReferences();
            var library = Compile("CanonicalLibrary_" + Guid.NewGuid().ToString("N"), emitted, references);
            references.Add(MetadataReference.CreateFromImage(library));
            var consumer = Compile("CanonicalConsumer_" + Guid.NewGuid().ToString("N"), """
                public static unsafe class Consumer
                {
                    public static int Run()
                    {
                        var canonical = DotCcFunctionPointers.add;
                        var runtime = DotCcFunctionPointers.abs;
                        if (canonical == null || runtime == null || DotCcLib.null_and_sentinel() != 1) return -1;
                        for (int i = 0; i < 30000; i++)
                        {
                            if (DotCcLib.get_first() != canonical || DotCcLib.get_second() != canonical) return -2;
                            if (DotCcLib.get_runtime() != runtime || DotCcLib.get_runtime_second() != runtime) return -3;
                            if (DotCcLib.accepts(canonical) != 1 || DotCcLib.other_accepts(canonical) != 1) return -4;
                            if (canonical(41) != 42 || runtime(-42) != 42) return -5;
                        }
                        System.GC.Collect();
                        System.GC.WaitForPendingFinalizers();
                        System.GC.Collect();
                        return DotCcFunctionPointers.add == canonical && DotCcLib.get_second() == canonical ? canonical(41) : -6;
                    }
                }
                """, references);
            var context = new AssemblyLoadContext("dotcc-canonical-" + Guid.NewGuid().ToString("N"), isCollectible: false);
            context.LoadFromStream(new MemoryStream(library));
            var assembly = context.LoadFromStream(new MemoryStream(consumer));
            assembly.GetType("Consumer", true)!.GetMethod("Run")!.Invoke(null, null).ShouldBe(42);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}

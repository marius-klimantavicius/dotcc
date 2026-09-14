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
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Static_local_array_lists_retain_independent_storage_through_gc(bool objectLink)
    {
        var fixture = FixtureRunner.Discover().Single(row => row.name == "static-local-array-list");
        var fragment = Path.Combine(Path.GetTempPath(), "dotcc-static-array-list-" + Guid.NewGuid().ToString("N") + ".cs");
        try
        {
            string emitted;
            if (objectLink)
            {
                File.WriteAllText(fragment, Compiler.EmitObject(fixture.sources.Single()));
                emitted = Compiler.LinkObjects(new[] { fragment }, emit:EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp(fixture.sources, emit:EmitMode.ManagedLib);
            var references = RuntimeReferences();
            var library = Compile("StaticArrays_" + Guid.NewGuid().ToString("N"), emitted, references);
            references.Add(MetadataReference.CreateFromImage(library));
            var consumer = Compile("StaticArraysConsumer_" + Guid.NewGuid().ToString("N"), """
                public static unsafe class Consumer {
                    public static int Run() {
                        int* first = DotCcLib.storage(0);
                        int* second = DotCcLib.storage(1);
                        if (first == second || first[0] != 0 || second[3] != 0) return -1;
                        first[0] = 41; second[3] = 99;
                        for (int pass = 0; pass < 3; ++pass) {
                            System.GC.Collect(2, System.GCCollectionMode.Forced, true, true);
                            System.GC.WaitForPendingFinalizers();
                            if (DotCcLib.storage(0) != first || DotCcLib.storage(1) != second) return -2;
                            if (first[0] != 41 || second[3] != 99 || DotCcLib.separate() != 0) return -3;
                        }
                        return DotCcLib.initialized() == 241 && DotCcLib.tick() == 8 && DotCcLib.tick() == 10 ? 42 : -4;
                    }
                }
                """, references);
            var context = new AssemblyLoadContext("static-array-lists-" + Guid.NewGuid().ToString("N"), isCollectible:false);
            context.LoadFromStream(new MemoryStream(library));
            context.LoadFromStream(new MemoryStream(consumer)).GetType("Consumer", true)!.GetMethod("Run")!.Invoke(null, null).ShouldBe(42);
        }
        finally { File.Delete(fragment); }
    }
}

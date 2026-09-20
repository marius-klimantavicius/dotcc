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
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Explicit_thread_local_arrays_keep_distinct_pinned_storage_through_gc(bool objectLink, bool blockScope)
    {
        var fixture = FixtureRunner.Discover().Single(row => row.name == (blockScope ? "tls-block-static" : "tls-explicit-arrays"));
        var fragment = Path.Combine(Path.GetTempPath(), "dotcc-tls-array-" + Guid.NewGuid().ToString("N") + ".cs");
        try
        {
            string emitted;
            if (objectLink)
            {
                File.WriteAllText(fragment, Compiler.EmitObject(fixture.sources.Single()));
                emitted = Compiler.LinkObjects(new[] { fragment }, emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp(fixture.sources, emit: EmitMode.ManagedLib);
            var references = RuntimeReferences();
            var libraryImage = Compile("TlsArrayLibrary_" + Guid.NewGuid().ToString("N"), emitted, references);
            references.Add(MetadataReference.CreateFromImage(libraryImage));
            var consumerImage = Compile("TlsArrayConsumer_" + Guid.NewGuid().ToString("N"), """
                public static unsafe class Consumer
                {
                    private static void Worker(int index, System.Threading.Barrier barrier, nint[] addresses, int[] errors)
                    {
                        ulong* pointer = DotCcLib.get_tls();
                        if (pointer == null) { errors[index] = 1; barrier.RemoveParticipant(); return; }
                        if (pointer[0] != 0 || pointer[1] != 0) errors[index] = 2;
                        ulong value = (ulong)(100 + index * 10);
                        DotCcLib.set_tls(value);
                        addresses[index] = (nint)pointer;
                        if (!barrier.SignalAndWait(System.TimeSpan.FromSeconds(10))) { errors[index] = 3; return; }
                        for (int pass = 0; pass < 3; ++pass)
                        {
                            System.GC.Collect(2, System.GCCollectionMode.Forced, true, true);
                            System.GC.WaitForPendingFinalizers();
                            if (DotCcLib.get_tls() != pointer || pointer[0] != value || pointer[1] != value + 1 || DotCcLib.check_tls(value) != 1)
                                errors[index] = 4;
                        }
                        if (!barrier.SignalAndWait(System.TimeSpan.FromSeconds(10))) errors[index] = 5;
                    }
                    public static int Run()
                    {
                        ulong* main = DotCcLib.get_tls();
                        if (main == null || main[0] != 0 || main[1] != 0) return -1;
                        DotCcLib.set_tls(3);
                        var addresses = new nint[2];
                        var errors = new int[2];
                        using var barrier = new System.Threading.Barrier(2);
                        var first = new System.Threading.Thread(() => Worker(0, barrier, addresses, errors)) { IsBackground = true };
                        var second = new System.Threading.Thread(() => Worker(1, barrier, addresses, errors)) { IsBackground = true };
                        first.Start(); second.Start();
                        if (!first.Join(30000) || !second.Join(30000)) return -2;
                        if (errors[0] != 0 || errors[1] != 0) return -3;
                        if (addresses[0] == addresses[1] || addresses[0] == (nint)main || addresses[1] == (nint)main) return -4;
                        System.GC.Collect(2, System.GCCollectionMode.Forced, true, true);
                        return DotCcLib.get_tls() == main && main[0] == 3 && main[1] == 4 && DotCcLib.check_tls(3) == 1 ? 42 : -5;
                    }
                }
                """, references);
            var context = new AssemblyLoadContext("dotcc-tls-array-" + Guid.NewGuid().ToString("N"), isCollectible: false);
            context.LoadFromStream(new MemoryStream(libraryImage));
            var consumer = context.LoadFromStream(new MemoryStream(consumerImage));
            consumer.GetType("Consumer", true)!.GetMethod("Run")!.Invoke(null, null).ShouldBe(42);
        }
        finally { File.Delete(fragment); }
    }
}

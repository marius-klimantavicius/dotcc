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
    public void Thread_local_scalar_addresses_survive_gc_and_differ_between_threads(bool objectLink)
    {
        var source = Path.Combine(Path.GetTempPath(), "dotcc-tls-address-" + Guid.NewGuid().ToString("N") + ".c");
        var fragment = Path.ChangeExtension(source, ".cs");
        File.WriteAllText(source, "_Thread_local int value; int *address(void) { return &value; }");
        try
        {
            string emitted;
            if (objectLink)
            {
                File.WriteAllText(fragment, Compiler.EmitObject(source));
                emitted = Compiler.LinkObjects(new[] { fragment }, emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp(new[] { source }, emit: EmitMode.ManagedLib);

            var references = RuntimeReferences();
            var libraryImage = Compile("TlsAddressLibrary_" + Guid.NewGuid().ToString("N"), emitted, references);
            references.Add(MetadataReference.CreateFromImage(libraryImage));
            var consumerImage = Compile("TlsAddressConsumer_" + Guid.NewGuid().ToString("N"), """
                public static unsafe class Consumer
                {
                    private static void Worker(int index, System.Threading.Barrier barrier, nint[] addresses, int[] errors)
                    {
                        int* pointer = DotCcLib.address();
                        *pointer = 100 + index;
                        addresses[index] = (nint)pointer;
                        if (!barrier.SignalAndWait(System.TimeSpan.FromSeconds(10))) { errors[index] = 1; return; }
                        for (int pass = 0; pass < 3; ++pass)
                        {
                            System.GC.Collect(2, System.GCCollectionMode.Forced, true, true);
                            System.GC.WaitForPendingFinalizers();
                            if (DotCcLib.address() != pointer || *pointer != 100 + index) errors[index] = 2;
                        }
                        if (!barrier.SignalAndWait(System.TimeSpan.FromSeconds(10))) errors[index] = 3;
                    }

                    public static int Run()
                    {
                        int* main = DotCcLib.address();
                        *main = 42;
                        var addresses = new nint[2];
                        var errors = new int[2];
                        using var barrier = new System.Threading.Barrier(2);
                        var first = new System.Threading.Thread(() => Worker(0, barrier, addresses, errors));
                        var second = new System.Threading.Thread(() => Worker(1, barrier, addresses, errors));
                        first.Start(); second.Start();
                        if (!first.Join(30000) || !second.Join(30000)) return -1;
                        if (errors[0] != 0 || errors[1] != 0) return -2;
                        if (addresses[0] == addresses[1] || addresses[0] == (nint)main || addresses[1] == (nint)main) return -3;
                        System.GC.Collect(2, System.GCCollectionMode.Forced, true, true);
                        return DotCcLib.address() == main && *main == 42 ? 0 : -4;
                    }
                }
                """, references);
            var context = new AssemblyLoadContext("dotcc-tls-address-" + Guid.NewGuid().ToString("N"), isCollectible: false);
            context.LoadFromStream(new MemoryStream(libraryImage));
            var consumer = context.LoadFromStream(new MemoryStream(consumerImage));
            consumer.GetType("Consumer", true)!.GetMethod("Run")!.Invoke(null, null).ShouldBe(0);
        }
        finally { File.Delete(source); File.Delete(fragment); }
    }
}

using System;
using System.IO;
using System.Runtime.Loader;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed partial class ManagedLibraryTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void External_pointer_lookup_matches_direct_calls_and_runtime_fallback(bool link, bool shadowRuntime)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-external-pointers-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var source = Path.Combine(directory, "probe.c");
            File.WriteAllText(source, """
                typedef int (*Callback)(int);
                extern int host_callback(int);
                int abs(int);
                Callback get_host(void) { return host_callback; }
                Callback get_runtime(void) { return abs; }
                int exercise(void) {
                    Callback local = host_callback;
                    if (local != get_host()) return -1;
                    return host_callback(10) + local(20) + abs(-3) + get_runtime()(-4);
                }
                """);
            var options = new CSharpOutputOptions(NestTypes:true, Runtime:RuntimeProfile.C);
            string emitted;
            if (link)
            {
                var fragment = Path.ChangeExtension(source, ".o");
                File.WriteAllText(fragment, Compiler.EmitObject(source));
                emitted = Compiler.LinkObjects(new[] { fragment }, emit:EmitMode.ManagedLib, className:"Api", namespaceName:"Ownership", outputOptions:options);
            }
            else emitted = Compiler.EmitCSharp(new[] { source }, emit:EmitMode.ManagedLib, className:"Api", namespaceName:"Ownership", outputOptions:options);
            emitted += "\npublic static unsafe partial class Api { public static int host_callback(int x) => x + 1; "
                + (shadowRuntime ? "public static int abs(int x) => x + 100;" : "") + " }";
            var library = Compile("ExternalPointers_" + Guid.NewGuid().ToString("N"), emitted, RuntimeReferences());
            var context = new AssemblyLoadContext("external-pointers-" + Guid.NewGuid().ToString("N"), isCollectible:false);
            var assembly = context.LoadFromStream(new MemoryStream(library));
            assembly.GetType("Ownership.Api", true)!.GetMethod("exercise")!.Invoke(null, null).ShouldBe(shadowRuntime ? 225 : 39);
        }
        finally { Directory.Delete(directory, true); }
    }
}

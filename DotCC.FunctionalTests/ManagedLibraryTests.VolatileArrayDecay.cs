using System;
using System.IO;
using System.Runtime.Loader;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed partial class ManagedLibraryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Volatile_array_typedef_decay_computes_address_and_preserves_elements(bool objectLink)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-volatile-array-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "probe.c");
            File.WriteAllText(path, """
                typedef unsigned long pair[2];
                volatile pair global;
                unsigned long *address(void) { return (unsigned long *)global; }
                int check(void) {
                    global[0] = 19;
                    global[1] = 23;
                    return (int)(address()[0] + global[1]);
                }
                """);
            if (objectLink)
            {
                var obj = Path.ChangeExtension(path, ".o");
                File.WriteAllText(obj, Compiler.EmitObject(path));
                path = obj;
            }
            var generated = objectLink
                ? Compiler.LinkObjects(new[] { path }, emit: EmitMode.ManagedLib, className: "Api")
                : Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, className: "Api");
            var image = Compile("VolatileArray_" + Guid.NewGuid().ToString("N"), generated, RuntimeReferences());
            var assembly = new AssemblyLoadContext("volatile-array-" + Guid.NewGuid(), isCollectible: false)
                .LoadFromStream(new MemoryStream(image));
            assembly.GetType("Api")!.GetMethod("check")!.Invoke(null, null).ShouldBe(42);
        }
        finally { Directory.Delete(directory, true); }
    }
}

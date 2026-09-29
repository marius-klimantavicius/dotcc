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
    public void Native_header_postfix_annotations_preserve_extern_calls_and_addresses(bool objectLink)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-native-attributes-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "probe.c");
            File.WriteAllText(path, """
                extern int add_one(int value) __attribute__((__nothrow__, __leaf__)) __attribute__((__const__));
                int add_one(int value) { return value + 1; }
                int check(void) {
                    int (*callback)(int) = add_one;
                    return add_one(20) + callback(20);
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
            var image = Compile("NativeHeaderAttributes_" + Guid.NewGuid().ToString("N"), generated, RuntimeReferences());
            var assembly = new AssemblyLoadContext("native-attributes-" + Guid.NewGuid(), isCollectible: false)
                .LoadFromStream(new MemoryStream(image));
            assembly.GetType("Api")!.GetMethod("check")!.Invoke(null, null).ShouldBe(42);
        }
        finally { Directory.Delete(directory, true); }
    }
}

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
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Identical_internal_functions_keep_distinct_addresses_across_units(bool objectLink, bool externalSecond)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-static-identity-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var first = Path.Combine(directory, "first.c");
        var second = Path.Combine(directory, "second.c");
        File.WriteAllText(first, """
            typedef int (*Callback)(int);
            typedef void (*Erased)(void);
            static int same(int value) { return value + 1; }
            int lock(int value) { return value + 1; }
            Callback other_same(void);
            int check(void) { return same != other_same() && same(41) == 42 && other_same()(41) == 42
                && (Callback)(Erased)same == same && lock(41) == 42; }
            """);
        var secondSource = """
            typedef int (*Callback)(int);
            static int same(int);
            Callback other_same(void) { return same; }
            static int same(int value) { return value + 1; }
            """;
        File.WriteAllText(second, externalSecond ? secondSource.Replace("static int same", "int same") : secondSource);
        try
        {
            string emitted;
            if (objectLink)
            {
                var a = Path.ChangeExtension(first, ".cs");
                var b = Path.ChangeExtension(second, ".cs");
                File.WriteAllText(a, Compiler.EmitObject(first));
                File.WriteAllText(b, Compiler.EmitObject(second));
                emitted = Compiler.LinkObjects(new[] { a, b }, emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp(new[] { first, second }, emit: EmitMode.ManagedLib);
            emitted.ShouldContain("public static readonly delegate*<int, int> @lock = &");
            var library = Compile("StaticIdentity_" + Guid.NewGuid().ToString("N"), emitted, RuntimeReferences());
            var context = new AssemblyLoadContext("dotcc-static-identity-" + Guid.NewGuid().ToString("N"), isCollectible: false);
            var assembly = context.LoadFromStream(new MemoryStream(library));
            assembly.GetType("DotCcLib", true)!.GetMethod("check")!.Invoke(null, null).ShouldBe(1);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}

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
    public void Enum_constants_follow_C_narrowing_and_sign_conversions_at_ordinary_call_boundaries(bool objects)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-enum-constant-call-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var source = Path.Combine(directory, "probe.c");
            File.WriteAllText(source, """
                enum Values { wide=65537, negative=-1 };
                unsigned short narrow(unsigned short value) { return value; }
                unsigned long widen(unsigned long value) { return value; }
                short sign(short value) { return value; }
                int probe(void) {
                    enum Values variable=wide;
                    return narrow(wide)==1 && narrow(variable)==1 &&
                        narrow(negative)==65535 && widen(negative)==~0UL && sign(negative)==-1;
                }
                """);
            string generated;
            if (objects)
            {
                var obj = Path.ChangeExtension(source, ".o");
                File.WriteAllText(obj, Compiler.EmitObject(source));
                generated = Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib, className: "Api");
            }
            else generated = Compiler.EmitCSharp(new[] { source }, emit: EmitMode.ManagedLib, className: "Api");
            var image = Compile("EnumArgumentConstants_" + Guid.NewGuid().ToString("N"), generated, RuntimeReferences());
            var assembly = new AssemblyLoadContext("enum-argument-constants-" + Guid.NewGuid(), isCollectible: false)
                .LoadFromStream(new MemoryStream(image));
            assembly.GetType("Api", true)!.GetMethod("probe")!.Invoke(null, null).ShouldBe(1);
        }
        finally { Directory.Delete(directory, true); }
    }
}

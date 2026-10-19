using System;
using System.Collections.Generic;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class ContextNameOriginMetadataTests
{
    [Fact]
    public void Alias_metadata_cannot_bless_an_authored_reserved_tag()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-tagged-origin-" + Guid.NewGuid().ToString("N") + ".c");
        var obj = path + ".o";
        var name = AnonymousTypedefNames.TypePrefix + new string('A', 64);
        File.WriteAllText(path, "struct " + name + " { int value; }; int probe(struct " + name + " *p) { return p->value; }");
        try
        {
            var text = Compiler.EmitObject(path, outputOptions: new(InstanceMethods: true));
            // This is a negative object-format fixture, not a product patch.
            // An alias record alone must not override the real tagged origin.
            text += AnonymousTypedefNames.Serialize(new Dictionary<string, string> { [name] = "Cell" });
            File.WriteAllText(obj, text);
            Should.Throw<CompileException>(() => Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib,
                outputOptions: new(Runtime: RuntimeProfile.C, InstanceMethods: true)))
                .Message.ShouldContain("reserves the __DotCc");
        }
        finally { File.Delete(path); File.Delete(obj); }
    }
}

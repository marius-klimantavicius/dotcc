#nullable enable
using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class InlineEnumTests
{
    [Fact]
    public void Tagged_enum_member_declares_its_type_and_enumerators()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-enum-member-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, """
            struct state { enum phase { NONE, READY = 7, DONE } phase; int value; };
            int main(void) { struct state s = {READY, 42}; enum phase p = DONE; return s.phase + p - 15; }
            """);
        try
        {
            var emitted = Compiler.EmitCSharp(new[] { path });
            emitted.ShouldContain("enum phase : int");
            emitted.ShouldContain("phase phase;");
            emitted.ShouldContain("READY = 7");
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Anonymous_enum_in_local_declaration_binds_integer_constants()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-enum-local-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, """
            int main(void) { enum { FULL, PSK, PSK_DHE } mode = PSK_DHE; return mode - 2; }
            """);
        try { Compiler.EmitCSharp(new[] { path }).ShouldContain("int mode = 2"); }
        finally { File.Delete(path); }
    }
}

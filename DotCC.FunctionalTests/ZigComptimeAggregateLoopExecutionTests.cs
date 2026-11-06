using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigComptimeAggregateLoopExecutionTests
{
    [Theory]
    [InlineData(false, "comptime-aggregate-loop.zig")]
    [InlineData(true, "comptime-aggregate-loop.zig")]
    [InlineData(false, "comptime-byte-payload.zig")]
    [InlineData(true, "comptime-byte-payload.zig")]
    [InlineData(false, "comptime-tuple-pointer.zig")]
    [InlineData(true, "comptime-tuple-pointer.zig")]
    public void Aggregate_slice_captures_and_element_pointers_execute_in_source_order(bool objects, string fixture)
        => new ZigSourceFileTypeExecutionTests()
            .Empty_file_structs_and_generic_module_reexports_keep_their_source_identity(objects, fixture);
    [Fact]
    public void Pointer_to_const_tuple_keeps_its_storage_immutable()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-const-tuple-" + Guid.NewGuid().ToString("N") + ".zig");
        File.WriteAllText(path, "const Pair = struct { u8, u8 }; pub fn main() u8 { const pair: Pair = .{ 20, 22 }; const pointer = &pair; pointer[0] = 21; return pointer[0]; }");
        try { Should.Throw<CompileException>(() => Compiler.EmitCSharp([path])).Message.ShouldContain("cannot assign"); }
        finally { File.Delete(path); }
    }

}

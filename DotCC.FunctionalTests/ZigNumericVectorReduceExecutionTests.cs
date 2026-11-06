using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigNumericVectorReduceExecutionTests
{
    [Theory]
    [InlineData(false, "numeric-reduce-page.zig")]
    [InlineData(true, "numeric-reduce-page.zig")]
    [InlineData(false, "numeric-reduce-integers.zig")]
    [InlineData(true, "numeric-reduce-integers.zig")]
    [InlineData(false, "numeric-reduce-widths.zig")]
    [InlineData(true, "numeric-reduce-widths.zig")]
    public void Integer_or_visits_every_lane_and_preserves_source_type_and_operand_effects(bool objects, string fixture)
        => new ZigSourceFileTypeExecutionTests()
            .Empty_file_structs_and_generic_module_reexports_keep_their_source_identity(objects, fixture);
}

using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigTypedEnumConstantExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Comptime_enum_seeds_keep_nominal_types_at_runtime_boundaries(bool objects)
        => new ZigSourceFileTypeExecutionTests()
            .Empty_file_structs_and_generic_module_reexports_keep_their_source_identity(objects, "comptime-enum-runtime.zig");
}

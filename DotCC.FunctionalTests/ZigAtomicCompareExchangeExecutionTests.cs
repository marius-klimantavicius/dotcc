using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigAtomicCompareExchangeExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Cas_preserves_observed_optional_and_ordered_operands(bool objects)
        => new ZigSourceFileTypeExecutionTests()
            .Empty_file_structs_and_generic_module_reexports_keep_their_source_identity(objects, "atomic-compare-exchange.zig");
}

using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigPointerCasExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Pointer_cas_keeps_null_payload_present_and_operands_ordered(bool objects)
        => new ZigSourceFileTypeExecutionTests()
            .Empty_file_structs_and_generic_module_reexports_keep_their_source_identity(objects, "pointer-cas.zig");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Allocator_packed_atomic_storage_preserves_return_old_and_source_bits(bool objects)
        => new ZigSourceFileTypeExecutionTests()
            .Empty_file_structs_and_generic_module_reexports_keep_their_source_identity(objects, "packed-atomic.zig");
}

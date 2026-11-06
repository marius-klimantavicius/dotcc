using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigStringPeerExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Different_string_extents_form_byte_slices_and_preserve_selected_effects(bool objects)
        => new ZigSourceFileTypeExecutionTests()
            .Empty_file_structs_and_generic_module_reexports_keep_their_source_identity(objects, "different-string-peers.zig");
}

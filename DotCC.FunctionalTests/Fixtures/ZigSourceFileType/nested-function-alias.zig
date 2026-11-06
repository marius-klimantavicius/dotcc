const provider = @import("nested-function-alias-provider.zig");
pub fn main() u8 {
    const os = u8;
    const abi = u16;
    _ = os;
    _ = abi;
    return provider.selected(40);
}

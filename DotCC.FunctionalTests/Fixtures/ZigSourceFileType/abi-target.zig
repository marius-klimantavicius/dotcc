const builtin = @import("builtin");
pub fn main() u8 {
    if (builtin.abi.isMusl()) return 1;
    if (!builtin.abi.isGnu()) return 2;
    return 42;
}

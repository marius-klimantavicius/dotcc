const api = @import("function-constant-api.zig");
pub fn main() u8 {
    api.check(true);
    return 42;
}

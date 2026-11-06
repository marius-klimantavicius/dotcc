const api = @import("api.zig");
comptime { _ = api; _ = @import("./api.zig"); }
pub fn main() u8 {
    if (api.identity() != 42) return 1;
    var value: c_int = 40;
    api.touch(&value);
    return @intCast(value);
}

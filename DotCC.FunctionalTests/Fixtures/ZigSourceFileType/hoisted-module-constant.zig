const provider = @import("hoisted-constant-provider.zig");
pub fn main() u8 {
    if (provider.count != 3) return 1;
    var values: [provider.count]u8 = undefined;
    values[0] = 40;
    values[1] = 1;
    values[2] = 1;
    if (@sizeOf(@TypeOf(values)) != 3) return 2;
    return values[0] + values[1] + values[2];
}

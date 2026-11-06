const Helpers = @import("file-namespace-provider.zig");
const Value = @import("file-namespace-value.zig");
pub fn main() u8 {
    var values: [2]Value = undefined;
    values[0] = .{ .bytes = .{ 1, 1, 1 }, .maybe = null };
    values[1] = .{ .bytes = .{ 1, 2, 3 }, .maybe = 4 };
    if (values[1].sum() != 10) return 1;
    if (@sizeOf(Value) < 3) return 2;
    return Helpers.add(values[0].sum());
}

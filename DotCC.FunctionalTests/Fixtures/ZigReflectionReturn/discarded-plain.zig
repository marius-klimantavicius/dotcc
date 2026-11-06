const schema = @import("metadata.zig");
fn Checked(comptime T: type) type {
    _ = schema.describe(T);
    return T;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const T = Checked(struct { value: u32 });
    return @intCast(@sizeOf(T));
}

const schema = @import("metadata.zig");
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const info = schema.describe(struct { value: u32 });
    return @intCast(info.field_names.len);
}

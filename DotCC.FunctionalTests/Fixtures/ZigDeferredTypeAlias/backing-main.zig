const api = @import("backing-api.zig");
fn read(tag: api.Tag.Backing) u8 {
    const value: api.Tag = @bitCast(tag);
    return if (value.flag) @intCast(value.value) else 1;
}
fn store(out: ?*api.Word, tag: api.Tag) void {
    if (out) |pointer| pointer.* = @bitCast(tag);
}
pub fn main() u8 {
    var word: api.Word = 0;
    store(&word, .{ .value = 42, .flag = true });
    store(null, .{ .value = 1, .flag = false });
    return read(word);
}

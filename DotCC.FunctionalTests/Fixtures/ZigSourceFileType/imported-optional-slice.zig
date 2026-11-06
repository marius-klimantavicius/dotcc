const options = @import("optional-slice-provider.zig");
const String = extern struct { ptr: [*]const u8, len: usize };
fn get(comptime which: u8, out: *String) void {
    const text = switch (which) { 0 => options.missing, 1 => options.present, else => options.empty };
    if (text) |b| {
        out.* = .{ .ptr = b.ptr, .len = b.len };
    } else {
        out.* = .{ .ptr = "", .len = 0 };
    }
}
fn missingDirect() usize {
    if (options.missing) |b| return b.len;
    return 999;
}
pub fn main() u8 {
    if (@TypeOf(options.missing) != ?[]const u8) return 1;
    if (missingDirect() != 999) return 2;
    var text: String = undefined;
    get(0, &text);
    if (text.len != 0) return 3;
    get(1, &text);
    if (text.len != 2 or text.ptr[1] != 'b') return 4;
    get(2, &text);
    if (text.len != 0) return 5;
    if ((options.present orelse "no").len != 2) return 6;
    return 42;
}

const Names = enum(u16) { first = 3, last = 500 };
const api = @import("imported.zig");

fn inspect(comptime candidate: ?type) u8 {
    if (candidate) |T| {
        if (T == Names) {
            if (@intFromEnum(T.first) != 3 or @intFromEnum(T.last) != 500) return 90;
            return 25;
        }
        if (T == api.Parent.Names) {
            return 37;
        }
        if (@typeInfo(T) == .pointer) {
            if (@typeInfo(T).pointer.size != .many or !@typeInfo(T).pointer.attrs.@"const") return 91;
            if (@typeInfo(T).pointer.sentinel()) |sentinel| {
                if (sentinel != 0) return 92;
            } else return 93;
            return 31;
        }
        return @intCast(@bitSizeOf(T));
    } else return 0;
}
fn forwarded(comptime candidate: ?type) u8 { return inspect(candidate); }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (forwarded(null) != 0 or forwarded(u5) != 5 or forwarded(u8) != 8) return 1;
    if (forwarded(Names) != 25 or forwarded([*:0]const u8) != 31) return 2;
    const selected: ?type = u5;
    const absent: ?type = null;
    if (forwarded(selected) != 5 or forwarded(absent) != 0) return 3;
    if (forwarded(api.Parent.Names) != 37) return 4;
    return 42;
}

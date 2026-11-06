const api = @import("optional-type-reexport.zig");
const Default = api.Maybe orelse u13;
fn bits(comptime T: type) u8 { return @intCast(@typeInfo(T).int.bits); }
pub fn main() u8 {
    const Local: ?type = if (true) u7 else @compileError("dead optional initializer");
    const LocalType = Local orelse @compileError("dead local fallback");
    const P = api.Pointer;
    if (bits(Default) != 13 or bits(api.Width) != 21 or bits(LocalType) != 7) return 1;
    if (@typeInfo(P).pointer.size != .many) return 2;
    if (@typeInfo(P).pointer.sentinel()) |sentinel| {
        if (sentinel != 0) return 3;
    } else return 3;
    const permission: api.Permissions = .write;
    if (@intFromEnum(permission) != 128 or @sizeOf(api.Permissions) != 2) return 4;
    const ordinary: ?u8 = null;
    const present: ?u8 = 23;
    if ((ordinary orelse 19) + (present orelse 99) != 42) return 5;
    return 42;
}

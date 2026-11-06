pub const selected_type = enum(u11) { selected = 42 };
const api = @import("optional-type-reexport.zig");
const Selected = api.Maybe orelse @compileError("present override");
pub fn main() u8 {
    if (Selected != selected_type or api.Permissions != selected_type) return 1;
    const value: api.Permissions = .selected;
    if (@sizeOf(api.Permissions) != @sizeOf(selected_type)) return 2;
    return @intCast(@intFromEnum(value));
}

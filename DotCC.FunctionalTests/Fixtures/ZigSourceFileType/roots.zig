const api = @import("roots-api.zig");
comptime {
    @export(&api.used, .{ .name = "selected_entry" });
    @export(&api.ffi_only, .{ .name = "unselected_ffi_entry" });
}
pub fn main() u8 { return api.used(); }

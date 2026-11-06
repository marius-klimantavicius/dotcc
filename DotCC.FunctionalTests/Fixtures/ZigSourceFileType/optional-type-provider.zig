const root = @import("root");
const builtin = @import("builtin");
pub const Options = struct {
    const Local = u21;
    pub const Width: ?type = Local;
    pub const Pointer: ?type = [*:0]const u8;
    pub const Choice: ?type = if (@hasDecl(root, "selected_type")) root.selected_type else null;
};
pub const Permissions = Options.Choice orelse if (builtin.os.tag == .windows)
    @compileError("unselected platform")
else if (true) enum(u9) { read = 1, write = 128 } else @compileError("unselected fallback");
pub const Width = Options.Width orelse @compileError("present width has no fallback");
pub const Pointer = Options.Pointer orelse @compileError("present pointer has no fallback");

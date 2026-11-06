const None: ?type = null;
const Present: ?type = u17;
const Fallback = None orelse u21;
const Chosen = Present orelse @compileError("dead type fallback");
const Explicit = @as(?type, if (true) u19 else @compileError("dead explicit branch")) orelse @compileError("dead explicit fallback");
const Holder = struct {
    const Local = u21;
    pub const Value: ?type = Local;
};
pub fn main() u8 {
    const local_null: ?type = null;
    const local_present: ?type = [*c]u8;
    const Local = local_null orelse u3;
    const Owned = Holder.Value orelse @compileError("dead owned fallback");
    const Pointer = local_present orelse @compileError("dead pointer fallback");
    if (@typeInfo(Explicit).int.bits != 19) return 3;
    if (@typeInfo(Owned).int.bits != 21) return 4;
    if (@typeInfo(Fallback).int.bits != 21 or @typeInfo(Chosen).int.bits != 17) return 1;
    if (@typeInfo(Local).int.bits != 3 or @typeInfo(Pointer).pointer.size != .c) return 2;
    return 42;
}

fn consume(calls: *u8, value: u8) u8 { calls.* += 10; return value; }
const Profile = struct {
    const enabled = switch (@as(u8, 0)) { 0 => true, else => false };
    pub fn choose() u8 { return if (enabled) 42 else @compileError("UNTYPED_DEAD_ARM"); }
    pub fn inferred() u8 {
        const value = if (enabled) @as(u8, 42) else @compileError("TYPED_DEAD_ARM");
        return value;
    }
};
fn runtimeBool(calls: *u8) bool { calls.* += 1; return true; }
fn early(calls: *u8) u8 {
    defer calls.* += 1;
    return consume(calls, switch (@typeInfo(u8)) {
        .int => return 17,
        else => @compileError("UNTAKEN_SWITCH_RETURN"),
    });
}
fn blocked(calls: *u8) u8 {
    defer calls.* += 1;
    return consume(calls, switch (@typeInfo(u16)) {
        .int => |info| { if (info.bits == 16) return 25; return 1; },
        else => @compileError("UNTAKEN_SWITCH_BLOCK"),
    });
}
pub fn main() u8 {
    var calls: u8 = 0;
    const result = early(&calls) + blocked(&calls);
    if (calls != 2) return 1;
    if (Profile.choose() != 42 or Profile.inferred() != 42) return 2;
    const runtime_value: u8 = if (runtimeBool(&calls)) 42 else 0;
    if (runtime_value != 42 or calls != 3) return 3;
    return result;
}

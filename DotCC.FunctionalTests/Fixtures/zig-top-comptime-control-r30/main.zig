const api = @import("api.zig");
const lib = @This();
fn compute(value: u8) u8 {
    var offset: u8 = 0;
    inline for (0..2) |_| { offset += 1; }
    return value + offset;
}
comptime {
    if (@import("root") != lib) @compileError("ROOT_IDENTITY");
    if (compute(40) != 42) @compileError("CALL_RESULT");
    for (0..3) |index| {
        if (index > 2) @compileError("INLINE_UNTAKEN");
    }
}
export fn zig_top_identity_and_answer() c_int {
    if (!api.pointerIdentity()) return 1;
    if (api.aggregateAnswer() != 42) return 3;
    const first = &api.same;
    const second = &api.same;
    if (first != second) return 2;
    return first(40);
}
pub fn main() u8 { return @intCast(zig_top_identity_and_answer()); }

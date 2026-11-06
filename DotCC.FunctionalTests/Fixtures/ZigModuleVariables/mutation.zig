const state = @import("state.zig");
const repeated = @import("./state.zig");
const other = @import("other.zig");
pub fn main() u8 {
    if (!@hasDecl(state, "global") or !@hasDecl(state, "unused")) return 1;
    if (state.read() != 5 or other.read() != 11) return 2;
    state.add(2);
    repeated.add(3);
    if (state.global != 10 or repeated.global != 10) return 3;
    const p = &state.global;
    if (p != repeated.pointer()) return 4;
    p.* = 20;
    other.add(4);
    if (state.read() != 20 or other.global != 15) return 5;
    other.global = 17;
    const element = &state.values[1];
    element.* += 2;
    if (repeated.values[1] != 9) return 6;
    return @intCast(state.read() + other.read() + state.values[0] + 2);
}

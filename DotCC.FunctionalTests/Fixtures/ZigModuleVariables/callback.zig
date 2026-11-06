const state = @import("callback-state.zig");
const again = @import("./callback-state.zig");
pub fn main() u8 {
    var value: u32 = 9;
    state.bare(&value);
    again.direct(&value, 0);
    if (state.override != null) return 3;
    state.fallback(&value);
    state.invoke(9);
    if (state.count() != 0) return 1;
    state.set(&value);
    const callback_slot = &state.override;
    callback_slot.* = null;
    if (state.override != null) return 4;
    again.invoke(4);
    state.replace();
    state.invoke(13);
    if (again.count() != 2) return 2;
    return @intCast(value + again.count());
}

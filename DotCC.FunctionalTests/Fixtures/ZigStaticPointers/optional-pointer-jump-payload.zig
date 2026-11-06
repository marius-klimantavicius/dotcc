var value: u8 = 42;
var calls: u8 = 0;

fn maybe(present: bool) ?*u8 {
    calls += 1;
    return if (present) &value else null;
}

fn inspect(pointer: anytype) u8 {
    if (@typeInfo(@TypeOf(pointer)).pointer.size != .one) return 1;
    return pointer.*;
}

fn returnPath(present: bool) u8 {
    const pointer = maybe(present) orelse return 17;
    return inspect(pointer);
}

fn nestedPath(present: bool) u8 {
    return inspect(maybe(present) orelse return 17);
}

fn breakPath(present: bool) u8 {
    return result: {
        const pointer = maybe(present) orelse break :result 17;
        break :result inspect(pointer);
    };
}

comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    calls = 0;
    if (returnPath(true) != 42 or returnPath(false) != 17) return 1;
    if (nestedPath(true) != 42 or nestedPath(false) != 17) return 2;
    if (breakPath(true) != 42 or breakPath(false) != 17) return 3;
    if (calls != 6) return 4;
    return 42;
}

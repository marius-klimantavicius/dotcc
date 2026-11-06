const callbacks = @import("callbacks.zig");
fn obtain(state: *callbacks.State) callbacks.Handle {
    const vtable: u32 = 4;
    _ = vtable;
    return callbacks.make(state);
}
fn clobber(seed: u64) u64 {
    var values: [256]u64 = undefined;
    for (&values, 0..) |*value, index| value.* = seed + index;
    return values[255];
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var first: callbacks.State = .{ .bias = 35, .calls = 0 };
    var second: callbacks.State = .{ .bias = 40, .calls = 0 };
    const a = obtain(&first);
    if (a.vtable != &callbacks.vtable or a.vtable.cookie != 123456789) return 1;
    const b = obtain(&second);
    if (a.vtable != b.vtable or a.context == b.context) return 2;
    for (0..128) |index| {
        if (clobber(index) != index + 255) return 3;
        if (a.vtable != &callbacks.vtable or callbacks.make(&first).vtable != a.vtable) return 4;
        if (a.vtable.run(a.context, 7) != 42 or b.vtable.run(b.context, 2) != 42) return 5;
    }
    if (first.calls != 128 or second.calls != 128) return 6;
    return 42;
}

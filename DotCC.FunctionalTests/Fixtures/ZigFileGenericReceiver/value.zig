const Value = @import("Value.zig");
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const receiver: Value = .{ .value = 17 };
    const selected = receiver.add(u8, 25);
    if (receiver.add(u16, 25) != 42) return 1;
    var count: u8 = 0;
    for (0..receiver.add(usize, 25)) |_| count += 1;
    return if (selected == count) selected else 2;
}

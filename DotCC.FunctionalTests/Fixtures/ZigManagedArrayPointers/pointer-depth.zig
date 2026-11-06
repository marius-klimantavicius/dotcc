const Node = struct { value: u32 };
fn slots(first: **Node, second: **Node) [2]**Node { return .{ first, second }; }
fn update(values: []const **Node) void {
    for (values, 0..) |slot, index| slot.*.value = @as(u32, @intCast(20 + index * 2));
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var a: Node = .{ .value = 1 };
    var b: Node = .{ .value = 2 };
    var c: Node = .{ .value = 3 };
    var first: *Node = &a;
    var second: *Node = &b;
    var addresses = slots(&first, &second);
    update(&addresses);
    if (a.value != 20 or b.value != 22) return 1;
    if (@intFromPtr(addresses[0]) != @intFromPtr(&first) or
        @intFromPtr(addresses[1]) != @intFromPtr(&second)) return 2;
    var copy = addresses;
    copy[0] = &second;
    if (addresses[0] != &first or copy[0] != &second) return 3;
    const view: []**Node = addresses[0..2];
    view[0].* = &c;
    if (first != &c or addresses[0].*.value != 3 or copy[0].*.value != 22) return 4;
    @memcpy(copy[0..2], view);
    if (copy[0] != &first or copy[1] != &second) return 5;
    view[0].*.value = 20;
    if (c.value != 20 or a.value != 20) return 6;
    return @intCast(c.value + b.value);
}

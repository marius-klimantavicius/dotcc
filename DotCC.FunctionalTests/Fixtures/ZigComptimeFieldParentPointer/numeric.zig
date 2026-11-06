const Child = struct { value: u64 };
const Parent = struct { count: u64, child: Child };
const address: usize = address: {
    var parent: Parent = .{ .count = 0, .child = .{ .value = 1 } };
    break :address @intFromPtr(&parent.child);
};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) usize { return address; }

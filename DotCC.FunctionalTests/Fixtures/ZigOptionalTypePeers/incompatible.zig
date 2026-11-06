const First = struct { a: u8 };
const Second = struct { b: u8 };
fn incompatible(left: ?First, right: ?Second) void {
    const Peer = @TypeOf(left, right);
    _ = Peer;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    incompatible(.{ .a = 1 }, .{ .b = 2 });
    return 42;
}

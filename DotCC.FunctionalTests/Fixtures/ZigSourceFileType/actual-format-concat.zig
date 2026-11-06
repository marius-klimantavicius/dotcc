const std = @import("std");
const Handler = struct {
    const encoded_tn_key = &std.fmt.bytesToHex("TN", .upper);
    fn reply(buffer: []u8, name: []const u8) [:0]const u8 {
        return std.mem.printSentinel(buffer, "\x1bP1+r" ++ encoded_tn_key ++ "={X}\x1b\\", .{name}, 0) catch unreachable;
    }
};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var buffer: [32]u8 = @splat(0xA5);
    const name: []const u8 = "\xAB\xCD";
    const reply = Handler.reply(buffer[1 .. buffer.len - 1], name);
    const expected: []const u8 = "\x1bP1+r544E=ABCD\x1b\\";
    if (!std.mem.eql(u8, reply, expected)) return 1;
    if (reply.ptr[reply.len] != 0 or buffer[0] != 0xA5 or buffer[buffer.len - 1] != 0xA5) return 2;
    return 42;
}

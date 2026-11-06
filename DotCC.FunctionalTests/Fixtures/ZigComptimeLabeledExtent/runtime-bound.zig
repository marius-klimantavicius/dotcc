var runtime_count: usize = 5;
const bytes: []const u8 = data: {
    var buffer: [runtime_count]u8 = @splat('a');
    break :data buffer[0..];
};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return @intCast(bytes.len); }

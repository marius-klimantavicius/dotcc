var storage: u32 = 41;
const result: u32 = blk: {
    const address = &storage;
    break :blk address.*;
};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return @intCast(result); }

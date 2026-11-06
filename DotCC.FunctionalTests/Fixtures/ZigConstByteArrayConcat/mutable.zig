var input: u8 = 1;
fn first(comptime text: []const u8) u8 { return text[0]; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var bytes: [2]u8 = @splat('a');
    bytes[0] = input;
    const text = bytes ++ "!";
    return first(text);
}

var value: u8 = 0;
fn key(input: u8) []const u8 { return if (input == 0) "a" else "b"; }
fn length(comptime text: []const u8) u8 { return @intCast(text.len); }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return length(key(value)); }

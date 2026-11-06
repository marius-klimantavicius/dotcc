const Tag = enum { number, empty };
fn consume(value: union(Tag) { number: u8 }) u8 { return value.number; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return consume(.{ .number = 42 }); }

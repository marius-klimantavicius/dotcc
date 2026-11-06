const Record = struct { T: type, marker: u8 };
var input: u8 = 1;
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const value: Record = .{ .T = u8, .marker = input };
    return value.marker;
}

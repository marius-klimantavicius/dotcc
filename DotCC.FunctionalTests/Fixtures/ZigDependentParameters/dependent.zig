const Owner = struct { first: u8, second: u16 };
const Field = enum(u8) { first, second, missing };

fn Box(comptime which: Field, comptime value: @FieldType(Owner, @tagName(which))) type {
    return struct {
        pub const selected = which;
        pub const saved: @FieldType(Owner, @tagName(which)) = value;
    };
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const which: u16 = 1000;
    const Wide = Box(.second, which);
    const Small = Box(.first, @as(u8, 7));
    if (Wide.saved != 1000 or Small.saved != 7) return 1;
    if (Wide.selected != .second or Small.selected != .first) return 2;
    if (@bitSizeOf(@TypeOf(Wide.saved)) != 16 or @bitSizeOf(@TypeOf(Small.saved)) != 8) return 3;
    return 42;
}

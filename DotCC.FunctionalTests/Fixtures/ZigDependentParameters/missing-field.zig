const Owner = struct { first: u8 };
const Field = enum(u8) { first, missing };
fn Box(comptime which: Field, comptime value: @FieldType(Owner, @tagName(which))) type {
    return struct { pub const saved = value; };
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { const Bad = Box(.missing, 7); return Bad.saved; }

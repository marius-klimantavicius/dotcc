const RGB = packed struct(u24) { r: u8, g: u8, b: u8 };
const Owner = struct { before: u8, palette: [4]RGB, after: u8 };
const Borrow = struct { palette: *const [4]RGB };
var effects: u32 = 0;
fn owner(pointer: *Owner) *Owner { effects = effects * 10 + 1; return pointer; }
fn mutate(pointer: *Owner) u32 { effects = effects * 10 + 2; pointer.palette[0].r = 40; return 2; }
fn read(palette: *const [4]RGB, extra: u32) u32 { return palette[0].r + extra; }

comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    effects = 0;
    var value: Owner = .{ .before = 77, .after = 88, .palette = .{
        .{ .r = 1, .g = 2, .b = 3 }, .{ .r = 4, .g = 5, .b = 6 },
        .{ .r = 7, .g = 8, .b = 9 }, .{ .r = 10, .g = 11, .b = 12 },
    } };
    const snapshot = value.palette;
    const borrowed: Borrow = .{ .palette = &owner(&value).palette };
    if (effects != 1 or @intFromPtr(borrowed.palette) != @intFromPtr(&value.palette)) return 1;
    if (borrowed.palette[3].b != 12 or value.before != 77 or value.after != 88) return 2;
    effects = 0;
    const result = read(&owner(&value).palette, mutate(&value));
    if (effects != 12 or result != 42 or borrowed.palette[0].r != 40) return 3;
    if (snapshot[0].r != 1 or snapshot[3].b != 12) return 4;
    return @intCast(result);
}

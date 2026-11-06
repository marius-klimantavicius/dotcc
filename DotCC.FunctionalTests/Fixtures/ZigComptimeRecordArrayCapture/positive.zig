const Entry = struct { name: []const u8, T: type, marker: u8 };
fn create(comptime name: []const u8, comptime T: type, comptime marker: u8) Entry {
    return .{ .name = name, .T = T, .marker = marker };
}
const entries = [_]Entry{ create("a\x00x", u8, 3), create("wide", u16, 5) };
var trace: u8 = 0;
fn touch(value: u8) void { trace = trace * 10 + value; }
fn width(comptime entry: Entry) usize { return @sizeOf(entry.T); }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var total: usize = 0;
    inline for (entries) |entry| {
        if (entry.marker == 3) {
            if (entry.name.len != 3 or entry.name[0] != 'a' or entry.name[1] != 0 or entry.name[2] != 'x') return 1;
        } else {
            if (entry.name.len != 4 or entry.name[0] != 'w') return 2;
        }
        total += width(entry);
        touch(entry.marker);
    }
    if (total != 3 or trace != 35) return 3;
    return 42;
}

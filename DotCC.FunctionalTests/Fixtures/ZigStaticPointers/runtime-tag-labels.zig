const Small = enum(u8) { low = 0, high = 255 };
const Mid = enum(u16) { low = 0, high = 65535 };
const Signed = enum(i16) { negative = -7, positive = 1 };
const Wide = enum(u32) { high = 4294967295 };
const Huge = enum(u64) { high = 1 << 40 };
var calls: u8 = 0;
fn next() Mid { calls += 1; return .high; }
fn name(value: anytype) []const u8 { return @tagName(value); }
fn same(left: []const u8, right: []const u8) bool {
    if (left.len != right.len) return false;
    var i: usize = 0;
    while (i < left.len) : (i += 1) if (left[i] != right[i]) return false;
    return true;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (!same(name(Small.high), "high")) return 1;
    if (!same(name(next()), "high") or calls != 1) return 2;
    if (!same(name(Signed.negative), "negative")) return 3;
    if (!same(name(Wide.high), "high")) return 4;
    if (!same(name(Huge.high), "high")) return 5;
    return 42;
}

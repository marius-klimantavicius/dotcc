const E = enum(i16) { low = -7, @"with space" = 31, high = 1000, _ };
const U = union(enum) { number: u8, empty: void };
var calls: u8 = 0;
fn next() E { calls += 1; return .@"with space"; }
fn name(value: E) [:0]const u8 { return @tagName(value); }
fn unionName(value: U) [:0]const u8 { return @tagName(value); }
fn same(a: []const u8, b: []const u8) bool {
    if (a.len != b.len) return false;
    for (a, b) |x, y| { if (x != y) return false; }
    return a.ptr[a.len] == 0;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    calls = 0;
    if (!same(name(.low), "low") or !same(name(.high), "high")) return 1;
    if (!same(@tagName(next()), "with space") or calls != 1) return 2;
    if (!same(unionName(.{ .number = 3 }), "number") or !same(unionName(.empty), "empty")) return 3;
    return 42;
}

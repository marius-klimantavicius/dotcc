const std = @import("std");
const Item = enum { BB, A, CCC, DDD, @"z-z" };
const map = std.StaticStringMap(Item).initEnum();
comptime { @export(&answer, .{ .name = "answer" }); }
fn matches(name: []const u8, expected: Item) bool {
    const actual = map.get(name) orelse return false;
    return @backingInt(actual) == @backingInt(expected);
}
pub fn answer() callconv(.c) u8 {
    if (!matches("A", .A) or !matches("BB", .BB) or !matches("CCC", .CCC)) return 1;
    if (!matches("DDD", .DDD) or !matches("z-z", .@"z-z")) return 2;
    if (map.get("a") != null or map.get("") != null or map.get("FFFF") != null) return 3;
    if (map.min_len != 1 or map.max_len != 3) return 4;
    const names = @typeInfo(Item).@"enum".field_names;
    const copy = names;
    if (copy.len != 5 or copy[0].len != 2 or copy[4][1] != '-') return 5;
    return 42;
}

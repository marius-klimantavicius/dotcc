const Tag = enum(u16) { pair = 250, failed = 11, empty = 3, number = 39, impossible = 77 };
const Pair = struct { left: u32, right: u32 };
const Failure = error{Failed};
const Value = @Union(.auto, Tag,
    &.{ "pair", "failed", "empty", "number", "impossible" },
    &.{ Pair, Failure!u64, void, u64, noreturn }, &@splat(.{}));
const EmptyTag = enum(u8) { last = 19, first = 3 };
const Empty = @Union(.auto, EmptyTag, &.{ "last", "first" }, &.{ void, void }, &@splat(.{}));
fn good() Failure!u64 { return 40; }
fn bad() Failure!u64 { return error.Failed; }
var calls: u8 = 0;
fn effect() void { calls += 1; }
fn voidConstructorControl() type {
    const action = @unionInit(Empty, "first", undefined);
    _ = action;
    return u8;
}
fn read(value: Value) u64 {
    switch (value) {
        .pair => |p, tag| { if (tag != .pair or @TypeOf(tag) != Tag) return 1; return p.left + p.right; },
        .failed => |result| return result catch 42,
        .number => |n| return n,
        .empty => return 0,
        .impossible => unreachable,
    }
}
pub fn main() u8 {
    const Answer = voidConstructorControl();
    if (@sizeOf(Answer) != 1) return 18;
    if (@typeInfo(Value) != .@"union") return 1;
    const info = @typeInfo(Value).@"union";
    if (info.tag_type.? != Tag or info.field_names.len != 5) return 2;
    if (info.field_names[0][0] != 'p' or info.field_types[0] != Pair or info.field_types[4] != noreturn) return 3;
    if (@typeInfo(info.field_types[1]) != .error_union) return 14;
    if (info.field_types[2] != void) return 24;
    if (@sizeOf(Value) != 24 or @alignOf(Value) != 8) return 5;
    if (@sizeOf(Empty) != 1 or @alignOf(Empty) != 1) return 6;
    var pair = @unionInit(Value, "pair", Pair{ .left = 20, .right = 21 });
    switch (pair) { .pair => |*p| p.right += 1, else => return 7 }
    if (read(pair) != 42 or @as(Tag, pair) != .pair or @intFromEnum(@as(Tag, pair)) != 250) return 8;
    const success = @unionInit(Value, "failed", good());
    const failure: Value = .{ .failed = bad() };
    if (read(success) != 40 or read(failure) != 42) return 9;
    const number: Value = .{ .number = 42 };
    const empty = @unionInit(Value, "empty", {});
    if (read(number) != 42 or read(empty) != 0 or @intFromEnum(@as(Tag, empty)) != 3) return 10;
    const effectful = @unionInit(Value, "empty", effect());
    if (read(effectful) != 0 or calls != 1) return 15;
    const literal_effect: Value = .{ .empty = effect() };
    if (read(literal_effect) != 0 or calls != 2) return 16;
    const first: Empty = .first;
    const last = @unionInit(Empty, "last", {});
    if (@as(EmptyTag, first) != .first or @intFromEnum(@as(EmptyTag, last)) != 19) return 11;
    var values: [2]Value = .{ pair, number };
    if (@intFromPtr(&values[1]) - @intFromPtr(&values[0]) != @sizeOf(Value)) return 12;
    return 42;
}

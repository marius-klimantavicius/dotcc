const Tag = enum(u16) { payload = 128, empty = 65280 };
const Alias = Tag;
const Value = union(Tag) { payload: u32, empty };
const Viewport = union(enum) { active, top, pin };
fn sameViewport(left: anytype, right: anytype) bool {
    const T = @TypeOf(left, right);
    if (T != Viewport) return false;
    const a: T = left;
    const b: T = right;
    return a == .active and b == .active;
}
var order: u32 = 0;
var current: Value = .{ .payload = 17 };
fn value() Value { order = order * 10 + 1; return .{ .payload = 3735928559 }; }
fn tag() Alias { order = order * 10 + 2; return .payload; }
fn reflected(input: Value) bool {
    const UnionTagType = @typeInfo(Value).@"union".tag_type.?;
    return input == @field(UnionTagType, "payload");
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const a: Value = .{ .payload = 999 };
    const b: Value = .empty;
    if (!(a == Tag.payload) or !(Tag.payload == a) or a != Alias.payload) return 1;
    if (!(b == Tag.empty) or !(Tag.empty == b) or b == Tag.payload) return 2;
    if (!reflected(a) or reflected(b)) return 3;
    order = 0;
    if (!(value() == tag()) or order != 12) return 4;
    order = 0;
    if (!(tag() == value()) or order != 21) return 5;
    order = 0;
    if (!(value() == blk: { order = order * 10 + 2; break :blk Tag.payload; }) or order != 12) return 6;
    current = .{ .payload = 17 };
    if (!(current == blk: { current = .empty; break :blk Tag.payload; })) return 7;
    if (current != .empty) return 8;
    if (@TypeOf(Value.empty) != Tag) return 9;
    const explicit: Value = Value.empty;
    if (explicit != .empty or @TypeOf(Value.empty, explicit) != Value) return 10;
    const viewport: Viewport = .active;
    if (!sameViewport(Viewport.active, viewport) or !sameViewport(viewport, Viewport.active)) return 11;
    return 42;
}

const Kind = enum(u8) {
    invalid,
    small,
    large,

    fn OutType(comptime self: Kind) type {
        return switch (self) { .invalid => void, .small => u8, .large => u16 };
    }
};
var calls: u8 = 0;
var picks: u8 = 0;
fn choose(kind: Kind) Kind { calls += 1; return kind; }
fn pick(comptime kind: Kind, value: kind.OutType()) u8 { picks += 1; return @intCast(value); }
fn dispatch(kind: Kind) u8 {
    return switch (choose(kind)) {
        .invalid => 0,
        inline else => |selected| pick(selected, 21),
    };
}
fn guarded(kind: Kind, value: ?u16) u8 {
    return switch (choose(kind)) {
        .invalid => 0,
        inline else => |selected| pick(selected, @intCast(value orelse return 6)),
    };
}
pub fn main() u8 {
    const a = dispatch(.small);
    const b = dispatch(.large);
    if (calls != 2 or dispatch(.invalid) != 0 or calls != 3) return 1;
    if (guarded(.large, 21) != 21 or guarded(.invalid, null) != 0 or guarded(.small, null) != 6) return 2;
    if (calls != 6 or picks != 3) return 3;
    return a + b;
}

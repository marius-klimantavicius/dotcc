const Value = union(enum) { empty, byte: u8, wide: u32 };
fn produce(calls: *u32) Value { calls.* += 1; return .{ .wide = 40 }; }
pub fn main() u8 {
    var calls: u32 = 0;
    const result: u32 = switch (produce(&calls)) {
        .byte => |v| v + 1,
        inline else => |v, tag| if (tag == .empty) 0 else v + 1,
    };
    if (calls != 1) return 1;
    return @intCast(result + calls);
}

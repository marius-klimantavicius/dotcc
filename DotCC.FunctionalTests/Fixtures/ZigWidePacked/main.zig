const E = enum(u32) { zero, high = 0xfedcba98 };
const S = enum(i7) { negative = -31, positive = 17 };
const Child = packed struct(u12) { head: u5 = 19, signed: i7 = -27 };
const Wide = packed struct(u128) {
    prefix: u35 = 0x54321,
    crossing: E = .high,
    signed: S = .negative,
    nested: Child = .{},
    tail: u42 = 0x32123456789,
};
const Outer = extern struct { before: u8, value: Wide, after: u8 };
const NestedWide = packed struct { child: Wide, };
const FullSigned = packed struct(i128) { value: i128 };
const SignedWide = packed struct(u128) { prefix: u3, value: i117, suffix: u8 };
fn copy(value: Wide) Wide { return value; }
fn change(value: *Wide) void {
    value.crossing = .zero;
    value.nested.signed = -11;
    value.nested.head += 2;
    value.signed = .positive;
}
pub fn main() u8 {
    if (@bitSizeOf(Wide) != 128 or @sizeOf(Wide) != 16 or @alignOf(Wide) != 16) return 1;
    if (@sizeOf(Outer) != 48 or @offsetOf(Outer, "value") != 16 or @offsetOf(Outer, "after") != 32) return 2;
    var bytes: [@sizeOf(Outer) + 32]u8 align(16) = undefined;
    @memset(&bytes, 0xa5);
    const outer: *Outer = @ptrCast(@alignCast(&bytes[16]));
    outer.* = .{ .before = 19, .value = .{}, .after = 23 };
    const original = copy(outer.value);
    if (original.crossing != .high or original.signed != .negative or original.nested.signed != -27) return 3;
    if (original.prefix != 0x54321 or original.tail != 0x32123456789 or original.nested.head != 19) return 4;
    change(&outer.value);
    if (outer.value.crossing != .zero or outer.value.signed != .positive) return 5;
    if (outer.value.nested.signed != -11 or outer.value.nested.head != 21) return 6;
    if (outer.value.prefix != original.prefix or outer.value.tail != original.tail or original.crossing != .high) return 7;
    if (outer.before + outer.after != 42 or bytes[15] != 0xa5 or bytes[@sizeOf(Outer) + 16] != 0xa5) return 8;
    if (@intFromPtr(&outer.after) - @intFromPtr(outer) != @offsetOf(Outer, "after")) return 9;
    var values: [2]Wide = .{ original, outer.value };
    values[1].nested.signed = -17;
    if (values[0].nested.signed != -27 or values[1].nested.signed != -17) return 10;
    if (@intFromPtr(&values[1]) - @intFromPtr(&values[0]) != 16) return 11;
    if (@intFromPtr(&values) % 16 != 0) return 12;
    var nested: NestedWide = .{ .child = original };
    nested.child.nested.signed = -19;
    if (nested.child.crossing != .high or nested.child.nested.signed != -19) return 13;
    const full: FullSigned = .{ .value = -42 };
    if (full.value != -42) return 14;
    var signed: SignedWide = .{ .prefix = 5, .value = -123456789, .suffix = 0xac };
    if (signed.value != -123456789) return 15;
    signed.value = -42;
    if (signed.value != -42 or signed.prefix != 5 or signed.suffix != 0xac) return 16;
    const raw: u128 = @bitCast(original);
    if (@as(u32, @truncate(raw >> 35)) != 0xfedcba98) return 17;
    const restored: Wide = @bitCast(raw);
    if (restored.nested.signed != -27 or restored.tail != original.tail) return 18;
    return 42;
}

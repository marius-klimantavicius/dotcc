const Alignment = enum(u3) { one = 0, eight = 3 };
const Signed = enum(i8) { negative = -12, positive = 30 };
const Wide = enum(u64) { high = 0x8000000000000000, maximum = 0xffffffffffffffff, _ };

fn alignmentValue(value: Alignment, effects: *u8) u8 {
    effects.* += 1;
    return @intFromEnum(value);
}
fn alignmentSeed(comptime value: Alignment, effects: *u8) u8 {
    const local: Alignment = value;
    return alignmentValue(value, effects) + alignmentValue(local, effects);
}
fn signedSeed(comptime value: Signed) Signed { return value; }
fn wideSeed(comptime value: Wide) Wide { return value; }
fn optionalSeed(comptime value: ?Alignment) ?Alignment { return value; }

pub fn answer() callconv(.c) u8 {
    var effects: u8 = 0;
    if (alignmentSeed(.eight, &effects) != 6 or effects != 2) return 1;
    if (@intFromEnum(signedSeed(.negative)) != -12) return 2;
    if (@intFromEnum(signedSeed(.positive)) != 30) return 3;
    if (@intFromEnum(wideSeed(.high)) != 0x8000000000000000) return 4;
    if (@intFromEnum(wideSeed(.maximum)) != 0xffffffffffffffff) return 5;
    if (optionalSeed(.eight).? != .eight) return 6;
    if (optionalSeed(null) != null) return 7;
    return 42;
}
pub fn main() u8 { return answer(); }

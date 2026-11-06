fn bits(value: anytype) u8 { return @typeInfo(@TypeOf(value)).int.bits; }
fn inspect(args: anytype) u8 { return bits(@field(args, "0")); }
fn forward(args: anytype) u8 {
    const saved = args;
    return inspect(saved);
}
fn pair(args: anytype) u8 {
    return bits(@field(args, "0")) + bits(@field(args, "1"));
}
const Small = enum(u3) { a, b };
const Byte = enum(u8) { a, b };
pub fn main() u8 {
    const one: u1 = 1;
    const three: u3 = 3;
    const seven: u7 = 7;
    const eight: u8 = 8;
    if (inspect(.{one}) != 1) return 1;
    if (inspect(.{seven}) != 7) return 2;
    if (inspect(.{eight}) != 8) return 3;
    if (inspect(.{three}) != 3) return 4;
    const saved = .{seven};
    if (inspect(saved) != 7 or forward(saved) != 7) return 5;
    if (forward(.{one}) != 1 or forward(.{eight}) != 8) return 6;
    if (pair(.{one, seven}) != 8 or pair(.{seven, one}) != 8) return 7;
    if (inspect(.{@as(i7, -1)}) != 7) return 8;
    if (inspect(.{@as(i8, -1)}) != 8) return 9;
    if (inspect(.{@backingInt(Small.b)}) != 3) return 10;
    if (inspect(.{@backingInt(Byte.b)}) != 8) return 11;
    return 42;
}

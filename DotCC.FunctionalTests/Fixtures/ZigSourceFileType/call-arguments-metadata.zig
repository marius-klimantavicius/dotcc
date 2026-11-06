const Bits = packed struct(u11) { lo: u5, hi: u6 };
fn mutate(p: *align(1) u11, fail: bool) ?u8 { p.* = 200; return if (fail) null else 1; }
fn generic(p: anytype, value: anytype, tail: u8) u16 {
    if (@bitSizeOf(@TypeOf(value)) != 11) @compileError("lost logical width");
    if ((@typeInfo(@TypeOf(p)).pointer.attrs.@"align" orelse 0) != 1) @compileError("lost alignment");
    if (@typeInfo(@TypeOf(p)).pointer.size != .one) @compileError("lost pointer size");
    return @as(u16, p.*) + @as(u16, value) + tail;
}
fn ordered(p: *align(1) u11, fail: bool) !u16 {
    return generic(p, @as(u11, p.*), mutate(p, fail) orelse return error.Missing);
}
fn packedValue(value: Bits, p: *align(1) u11, tail: u8) u16 {
    return @as(u16, @as(u11, @bitCast(value))) + @as(u16, p.*) + tail;
}
fn packedCall(value: Bits, p: *align(1) u11) !u16 {
    return packedValue(value, p, mutate(p, false) orelse return error.Missing);
}
fn narrowOptional(value: u11, fail: bool) ?u11 { return if (fail) null else value; }
fn narrowError(value: u11, fail: bool) !u11 { return if (fail) error.Narrow else value; }
fn width(value: anytype, tail: u8) u16 {
    if (@bitSizeOf(@TypeOf(value)) != 11) @compileError("lost unwrapped logical width");
    return @as(u16, value) + tail;
}
fn wrapped(p: *align(1) u11, fail: bool) !u16 {
    return width(narrowError(100, fail) catch return error.ErrorPayload, mutate(p, false) orelse return error.Missing);
}
fn wrappedOptional(p: *align(1) u11, fail: bool) !u16 {
    return width(narrowOptional(100, fail) orelse return error.OptionalPayload, mutate(p, false) orelse return error.Missing);
}
pub fn main() u8 {
    var value: u11 align(1) = 100;
    if ((ordered(&value, false) catch 0) != 301 or value != 200) return 1;
    value = 100;
    if (ordered(&value, true)) |_| { return 2; } else |err| { if (err != error.Missing or value != 200) return 3; }
    const bits: Bits = @bitCast(@as(u11, 801));
    value = 100;
    if ((packedCall(bits, &value) catch 0) != 1002 or value != 200) return 4;
    if ((wrapped(&value, false) catch 0) != 101) return 5;
    if (wrapped(&value, true)) |_| { return 6; } else |err| { if (err != error.ErrorPayload) return 7; }
    if ((wrappedOptional(&value, false) catch 0) != 101) return 8;
    if (wrappedOptional(&value, true)) |_| { return 9; } else |err| { if (err != error.OptionalPayload) return 10; }
    return 42;
}

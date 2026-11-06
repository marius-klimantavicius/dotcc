fn exercise(comptime T: type, start: T, amount: T, added: T, subtracted: T, multiplied: T) bool {
    var value = start;
    value +%= amount;
    if (value != added) return false;
    value = start;
    value -%= amount;
    if (value != subtracted) return false;
    value = start;
    value *%= amount;
    return value == multiplied;
}
fn destination(trace: *u32, pointer: *u3) *u3 {
    trace.* = trace.* * 10 + 1;
    return pointer;
}
fn operand(trace: *u32, pointer: *u3) u3 {
    trace.* = trace.* * 10 + 2;
    pointer.* = 6;
    return 3;
}
fn index(trace: *u32) usize { trace.* = trace.* * 10 + 1; return 1; }
fn argument(trace: *u32) u3 { trace.* = trace.* * 10 + 2; return 3; }
const Packed = packed struct { low: u3, signed: i3, other: u2 };
fn compileTime(value: u3) u3 { var result = value; result +%= 3; result *%= 3; result -%= 1; return result; }
fn compileTimeAlias() u3 {
    var trace: u32 = 0;
    var value: u3 = 7;
    destination(&trace, &value).* +%= operand(&trace, &value);
    return value;
}
fn packedOperand(pointer: *Packed) u3 { pointer.low = 6; return 3; }
fn volatilePointer(pointer: *volatile u8) u8 { const old = pointer.*; pointer.* = old + 1; return pointer.*; }
fn volatileSlice(values: []volatile u8) u8 { const old = values[0]; values[0] = old + 1; return values[0]; }
pub fn main() u8 {
    const folded = comptime compileTime(7);
    const folded_alias = comptime compileTimeAlias();
    if (!exercise(u3, 7, 3, 2, 4, 5)) return 1;
    if (!exercise(i3, 3, 2, -3, 1, -2)) return 2;
    if (!exercise(i3, -4, 1, -3, 3, -4)) return 3;
    if (!exercise(u9, 511, 3, 2, 508, 509)) return 4;
    if (!exercise(i9, -256, 3, -253, 253, -256)) return 5;
    if (!exercise(u24, 16777215, 3, 2, 16777212, 16777213)) return 6;
    if (!exercise(i24, 8388607, 3, -8388606, 8388604, 8388605)) return 7;
    if (!exercise(u63, 9223372036854775807, 3, 2, 9223372036854775804, 9223372036854775805)) return 8;
    if (!exercise(i63, -4611686018427387904, 3, -4611686018427387901, 4611686018427387901, -4611686018427387904)) return 9;
    if (!exercise(u65, 36893488147419103231, 3, 2, 36893488147419103228, 36893488147419103229)) return 10;
    if (!exercise(i65, 18446744073709551615, 3, -@as(i65, 18446744073709551615) + 1, 18446744073709551612, 18446744073709551613)) return 11;
    if (!exercise(u8, 255, 3, 2, 252, 253)) return 12;
    if (!exercise(i8, 127, 3, -126, 124, 125)) return 13;
    var trace: u32 = 0;
    var value: u3 = 7;
    destination(&trace, &value).* +%= operand(&trace, &value);
    if (trace != 12 or value != 2) return 14;
    trace = 0;
    var values = [2]u3{ 4, 7 };
    values[index(&trace)] *%= argument(&trace);
    if (trace != 12 or values[0] != 4 or values[1] != 5) return 15;
    var turn: u3 = 6;
    var iterations: u8 = 0;
    while (iterations < 4) : (turn +%= 1) { iterations += 1; }
    if (turn != 2) return 16;
    var packed_value = Packed{ .low = 7, .signed = 3, .other = 2 };
    packed_value.low +%= 3;
    packed_value.signed *%= 2;
    if (packed_value.low != 2 or packed_value.signed != -2 or packed_value.other != 2) return 17;
    if (folded != 5) return 18;
    trace = 0;
    value = 2;
    destination(&trace, &value).* -%= operand(&trace, &value);
    if (trace != 12 or value != 7) return 19;
    trace = 0;
    value = 2;
    destination(&trace, &value).* *%= operand(&trace, &value);
    if (trace != 12 or value != 6) return 20;
    if (folded_alias != 2) return 21;
    packed_value.low = 2;
    packed_value.low +%= packedOperand(&packed_value);
    if (packed_value.low != 5 or packed_value.other != 2 or packed_value.signed != -2) return 22;
    comptime var wrapped: u3 = 6;
    inline for (0..4) |_| { wrapped +%= 1; }
    if (wrapped != 2) return 23;
    var volatile_value: u8 = 41;
    if (volatilePointer(&volatile_value) != 42 or volatile_value != 42) return 24;
    var volatile_values = [1]u8{41};
    if (volatileSlice(volatile_values[0..]) != 42 or volatile_values[0] != 42) return 25;
    return 42;
}

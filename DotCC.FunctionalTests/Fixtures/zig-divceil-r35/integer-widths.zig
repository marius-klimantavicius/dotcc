fn calc_i8(a: i8, b: i8) i8 { return @divCeil(a, b); }
fn calc_i16(a: i16, b: i16) i16 { return @divCeil(a, b); }
fn calc_i32(a: i32, b: i32) i32 { return @divCeil(a, b); }
fn calc_i64(a: i64, b: i64) i64 { return @divCeil(a, b); }
fn calc_i128(a: i128, b: i128) i128 { return @divCeil(a, b); }
fn calc_u8(a: u8, b: u8) u8 { return @divCeil(a, b); }
fn calc_u16(a: u16, b: u16) u16 { return @divCeil(a, b); }
fn calc_u32(a: u32, b: u32) u32 { return @divCeil(a, b); }
fn calc_u64(a: u64, b: u64) u64 { return @divCeil(a, b); }
fn calc_u128(a: u128, b: u128) u128 { return @divCeil(a, b); }
pub fn main() u8 {
    if (calc_i8(7, 3) != 3) return 1;
    if (calc_i8(-7, 3) != -2) return 1;
    if (calc_i8(7, -3) != -2) return 1;
    if (calc_i8(-7, -3) != 3) return 1;
    if (calc_i8(-6, 3) != -2) return 1;
    if (calc_i8(0, 3) != 0) return 1;
    if (calc_i8(127, 2) != 64) return 2;
    if (calc_i16(7, 3) != 3) return 1;
    if (calc_i16(-7, 3) != -2) return 1;
    if (calc_i16(7, -3) != -2) return 1;
    if (calc_i16(-7, -3) != 3) return 1;
    if (calc_i16(-6, 3) != -2) return 1;
    if (calc_i16(0, 3) != 0) return 1;
    if (calc_i16(32767, 2) != 16384) return 2;
    if (calc_i32(7, 3) != 3) return 1;
    if (calc_i32(-7, 3) != -2) return 1;
    if (calc_i32(7, -3) != -2) return 1;
    if (calc_i32(-7, -3) != 3) return 1;
    if (calc_i32(-6, 3) != -2) return 1;
    if (calc_i32(0, 3) != 0) return 1;
    if (calc_i32(2147483647, 2) != 1073741824) return 2;
    if (calc_i64(7, 3) != 3) return 1;
    if (calc_i64(-7, 3) != -2) return 1;
    if (calc_i64(7, -3) != -2) return 1;
    if (calc_i64(-7, -3) != 3) return 1;
    if (calc_i64(-6, 3) != -2) return 1;
    if (calc_i64(0, 3) != 0) return 1;
    if (calc_i64(9223372036854775807, 2) != 4611686018427387904) return 2;
    if (calc_i128(7, 3) != 3) return 1;
    if (calc_i128(-7, 3) != -2) return 1;
    if (calc_i128(7, -3) != -2) return 1;
    if (calc_i128(-7, -3) != 3) return 1;
    if (calc_i128(-6, 3) != -2) return 1;
    if (calc_i128(0, 3) != 0) return 1;
    if (calc_i128(170141183460469231731687303715884105727, 2) != 85070591730234615865843651857942052864) return 2;
    if (calc_u8(7, 3) != 3) return 1;
    if (calc_u8(0, 3) != 0) return 1;
    if (calc_u8(255, 2) != 128) return 2;
    if (calc_u16(7, 3) != 3) return 1;
    if (calc_u16(0, 3) != 0) return 1;
    if (calc_u16(65535, 2) != 32768) return 2;
    if (calc_u32(7, 3) != 3) return 1;
    if (calc_u32(0, 3) != 0) return 1;
    if (calc_u32(4294967295, 2) != 2147483648) return 2;
    if (calc_u64(7, 3) != 3) return 1;
    if (calc_u64(0, 3) != 0) return 1;
    if (calc_u64(18446744073709551615, 2) != 9223372036854775808) return 2;
    if (calc_u128(7, 3) != 3) return 1;
    if (calc_u128(0, 3) != 0) return 1;
    if (calc_u128(340282366920938463463374607431768211455, 2) != 170141183460469231731687303715884105728) return 2;
    const n = @divCeil(129, 64);
    const Data = [n]u8;
    if (@sizeOf(Data) != 3) return 3;
    return 42;
}

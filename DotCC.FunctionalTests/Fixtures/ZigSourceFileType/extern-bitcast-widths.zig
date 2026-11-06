extern "c" fn abs(value: c_int) c_int;
extern "c" fn labs(value: c_long) c_long;
pub fn main() u8 {
    const small: u32 = @bitCast(abs(@bitCast(@as(u32, 0xfffffff9))));
    const large: u64 = @bitCast(labs(@bitCast(@as(u64, 0xffffffffffffffdd))));
    return @intCast(small + large);
}

const Record = extern struct { first: u8, @"odd-name": u16, rest: u32 };
const Packed = packed struct(u16) { first: u3, @"odd-name": u5, rest: u8 };
fn byteOffset(comptime name: []const u8) usize { return @offsetOf(Record, name); }
pub fn main() u8 {
    var byte_sum: usize = 0;
    var bit_sum: usize = 0;
    inline for (.{ "first", "odd-name", "rest" }) |name| {
        byte_sum += @offsetOf(Record, name);
        bit_sum += @bitOffsetOf(Packed, name);
    }
    if (byte_sum != 6 or bit_sum != 11) return 1;
    if (byteOffset("odd-" ++ "name") != 2) return 1;
    return 42;
}

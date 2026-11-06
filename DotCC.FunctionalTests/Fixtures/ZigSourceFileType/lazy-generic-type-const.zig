fn Box(comptime T: type) type {
    return struct {
        value: T,
        pub const Bit = @Int(.unsigned, @typeInfo(T).int.bits);
        pub fn get(self: @This()) T { return self.value; }
        pub fn bit(self: @This(), index: Bit) T { _ = index; return self.value; }
    };
}
pub fn main() u8 {
    var byte: u8 = 35;
    const pointer: Box(*u8) = .{ .value = &byte };
    const integer: Box(u7) = .{ .value = 7 };
    const T = u64;
    _ = T;
    if (@typeInfo(Box(u7).Bit).int.bits != 7) return 1;
    return pointer.get().* + integer.get();
}

const Self = @This();
value: u8,
pub fn add(self: Self, comptime T: type, extra: T) T {
    return @as(T, self.value) + extra;
}
pub fn Result(comptime self: Self) type {
    return if (self.value == 17) u8 else u16;
}

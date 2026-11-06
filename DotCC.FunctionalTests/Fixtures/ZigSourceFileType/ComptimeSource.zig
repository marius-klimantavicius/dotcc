const Source = @This();
value: u8,
pub fn add(comptime self: Source, amount: u8) u8 {
    return self.value + amount;
}
pub fn constant(comptime self: Source) u8 {
    return self.value;
}
pub fn addRuntime(self: Source, amount: u8) u8 {
    return self.value + amount;
}

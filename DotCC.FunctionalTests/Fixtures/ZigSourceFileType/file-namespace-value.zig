bytes: [3]u8,
maybe: ?u8,
pub fn sum(self: @This()) u8 {
    return self.bytes[0] + self.bytes[1] + self.bytes[2] + (self.maybe orelse 0);
}

const Choice = enum(u8) { other = 42, special = 7, @"odd-name" = 9 };
fn field(comptime E: type, comptime name: []const u8) E {
    return @field(E, name);
}
pub fn main() u8 {
    if (@field(@TypeOf(Choice.special), "special") != .special) return 1;
    if (@field(@TypeOf(Choice.other), "other") != .other) return 2;
    if (@field(@TypeOf(Choice.special), "odd-name") != .@"odd-name") return 3;
    if (field(Choice, "special") != .special) return 4;
    return @intFromEnum(@field(@TypeOf(Choice.other), "other"));
}

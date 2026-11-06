const Tag = packed struct(u16) {
    pub const Backing = @typeInfo(@This()).@"struct".backing_integer.?;
    value: u15,
    flag: bool = false,
};
fn fill(output: []Tag.Backing, values: []const u15) void {
    for (values, output) |value, *slot| slot.* = @bitCast(Tag{ .value = value });
}
pub fn main() u8 {
    var output: [2]Tag.Backing = undefined;
    const values = [_]u15{ 11, 31 };
    fill(&output, &values);
    var copied: [2]Tag.Backing = undefined;
    for (output, &copied) |value, *slot| slot.* = @bitCast(@as(Tag, @bitCast(value)));
    if (copied[0] != 11 or copied[1] != 31) return 1;
    return @intCast(copied[0] + copied[1]);
}

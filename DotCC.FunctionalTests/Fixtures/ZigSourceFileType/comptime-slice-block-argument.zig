const Tag = packed struct(u16) {
    value: u15,
    flag: bool = false,
};
fn Table(comptime T: type, comptime keys: []const T) type {
    return struct {
        pub fn get(index: usize) T { return keys[index]; }
    };
}
const Values = Table(u16, keys: {
    var keys: [2]u16 = undefined;
    const values = [_]u15{ 11, 31 };
    for (values, &keys) |value, *key| key.* = @bitCast(Tag{ .value = value });
    const final = keys;
    break :keys &final;
});
pub fn main() u8 {
    if (Values.get(0) != 11 or Values.get(1) != 31) return 1;
    return @intCast(Values.get(0) + Values.get(1));
}

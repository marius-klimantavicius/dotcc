pub fn main() u8 {
    const V = @Vector(64, u8);
    var input: [64]u8 = undefined;
    var output: [64]u32 = undefined;
    for (&input, 0..) |*value, i| value.* = @intCast(i);
    const vector: V = @bitCast(input);
    if (@reduce(.Or, vector >= @as(V, @splat(0x80)))) return 1;
    inline for (0..64) |lane| output[lane] = vector[lane];
    for (output, 0..) |value, i| if (value != i) return 2;
    return 42;
}

const Style = enum { bar, block, underline, block_hollow };
const CStyle = enum(c_int) {
    bar = 0, block = 1, underline = 2, block_hollow = 3, _,
    fn convert(self: CStyle) ?Style {
        return switch (self) {
            .bar => .bar,
            .block => .block,
            .underline => .underline,
            .block_hollow => .block_hollow,
            else => null,
        };
    }
};
pub fn main() u8 {
    for (0..4) |i| {
        const raw: CStyle = @enumFromInt(i);
        if (@intFromEnum(raw.convert().?) != i) return 1;
    }
    const negatives = [_]c_int{ -1, 4, 2147483647, (-2147483647 - 1) };
    for (negatives) |i| {
        const raw: CStyle = @enumFromInt(i);
        if (raw.convert() != null) return 2;
    }
    return 42;
}

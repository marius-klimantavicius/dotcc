pub const Style = enum { bar, block, underline, block_hollow };
pub const CStyle = enum(c_int) {
    bar = 0, block = 1, underline = 2, block_hollow = 3, _,
    pub fn convert(self: CStyle) ?Style {
        return switch (self) {
            .bar => .bar,
            .block => .block,
            .underline => .underline,
            .block_hollow => .block_hollow,
            _ => null,
        };
    }
};

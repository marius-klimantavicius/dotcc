pub const Kind = enum(u8) {
    tiny = 5,
    wide = 9,
    pub fn InType(comptime self: Kind) type {
        return switch (self) { .tiny => u3, .wide => u21 };
    }
    pub fn Array(comptime self: Kind, comptime n: usize) type {
        return [n]self.InType();
    }
};

pub const formatter: bool = true;
pub const search: bool = false;

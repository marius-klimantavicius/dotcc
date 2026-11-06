pub const Kind = enum(u8) {
    first, second,
    pub const isOLD = isFirst;
    pub fn isFirst(self: Kind) bool { return self == .first; }
};

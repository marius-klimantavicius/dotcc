pub const Word = u16;
pub const Tag = packed struct(u16) {
    pub const Backing = @typeInfo(@This()).@"struct".backing_integer.?;
    value: u15,
    flag: bool,
};

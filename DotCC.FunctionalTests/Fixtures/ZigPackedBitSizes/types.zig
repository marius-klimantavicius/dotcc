pub const Flags8 = packed struct {
    a: bool, b: bool, c: bool, d: bool,
    e: bool, f: bool, g: bool, h: bool,
};
pub const Mixed8 = packed struct { a: u3, b: u5 };
pub const NonByte9 = packed struct { a: u3, b: u6 };
pub const Backed16 = packed struct(u16) { a: u8, b: u8 };
pub const Tag3 = enum(u3) { first, second, third };
pub const Nested16 = packed struct { inner: Mixed8, tag: Tag3, rest: u5 };
pub const Empty = packed struct {};

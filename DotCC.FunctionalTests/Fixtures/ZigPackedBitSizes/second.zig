const Backing = u5;
pub const Tag = enum(Backing) { zero, one };
pub const Bits = packed struct { tag: Tag, extra: u1 };

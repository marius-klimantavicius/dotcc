pub const Early = struct { kind: Operation.Tag };
pub const Operation = union(enum) {
    read: Read,
    write: Write,
    close,
    pub const Tag = @typeInfo(Operation).@"union".tag_type.?;
    pub const Read = struct { value: u8 };
    pub const Pending = struct { kind: Tag, value: u8 };
    pub const Write = struct { value: u16 };
};
pub const Selector = enum(u8) { first = 5, last = 9 };
pub const Explicit = union(Selector) {
    first: u8,
    last,
    pub const Payload = struct { tag: @typeInfo(Explicit).@"union".tag_type.?, value: u8 };
};

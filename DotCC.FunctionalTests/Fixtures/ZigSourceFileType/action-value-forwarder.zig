fn Payloads(comptime Union: type) type {
    return struct {
        pub const Tag = @typeInfo(Union).@"union".tag_type.?;
        pub fn Value(comptime tag: Tag) type { return @FieldType(Union, @tagName(tag)); }
    };
}
const Message = union(enum) {
    print: Print,
    text: []const u8,
    pointer: *u64,
    none,
    const helper = Payloads(@This());
    pub const Tag = helper.Tag;
    pub fn Value(comptime tag: Tag) type { return helper.Value(tag); }
    pub const Print = struct { cp: u21 };
};
fn interpret(comptime tag: Message.Tag, value: Message.Value(tag)) u8 {
    return switch (tag) {
        .print => @intCast(value.cp),
        .text => if (value.len == 2) 42 else 1,
        .pointer => @intCast(value.*),
        .none => 42,
    };
}
pub fn main() u8 {
    var n: u64 = 42;
    if (interpret(.print, .{ .cp = 42 }) != 42) return 1;
    if (interpret(.text, "hi") != 42) return 2;
    if (interpret(.pointer, &n) != 42) return 3;
    if (@sizeOf(Message.Value(.print)) != @sizeOf(Message.Print)) return 4;
    return interpret(.none, {});
}

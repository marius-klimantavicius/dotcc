pub const Flags = packed struct(u5) {
    a: bool = false,
    b: bool = false,
    c: bool = false,
    d: bool = false,
    e: bool = false,
    pub const disabled: Flags = .{};
    pub const enabled: Flags = .{ .a = true, .b = true, .c = true, .d = true, .e = true };
};
pub const Stack = struct {
    const len = 8;
    flags: [len]Flags = @splat(.disabled),
    index: u3 = 0,
};

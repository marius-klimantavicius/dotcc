pub fn Stats(comptime T: type) type {
    return struct {
        pub const info = blk: {
            const Data = struct { value: T };
            const Access = struct {
                fn read(value: Data) T { return value.value; }
            };
            const data: Data = .{ .value = 42 };
            break :blk Data{ .value = Access.read(data) };
        };
        pub const stamp: u8 = blk: {
            const Kind = enum(u8) { answer = 42 };
            const Handle = opaque {};
            break :blk @intCast(@intFromEnum(Kind.answer) + @sizeOf(*Handle) - @sizeOf(usize));
        };
    };
}

fn Wrap(comptime C: type) type { return struct { pub const Context = C; }; }
pub fn Box(comptime T: type, comptime N: usize) type {
    return Wrap(struct {
        const Self = @This();
        pub const Inner = struct { value: T };
        pub const width = @bitSizeOf(T);
        value: T,
        inner: Inner,
        bytes: [N]u8,
        pub fn answer(self: *const Self) u8 { return @intCast(self.value + self.inner.value + N); }
        pub fn unused() u8 { @compileError("R41_UNUSED_BODY"); }
    });
}
pub fn Identity(comptime T: type) type { return T; }
pub const First = Identity(struct { value: u8 });
pub const Second = Identity(struct { value: u8 });

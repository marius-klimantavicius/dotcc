fn Table(comptime Key: type) type {
    return struct {
        const Metadata = packed struct { fingerprint: u7 = 0, used: u1 = 0 };
        const Narrow = u7;
        const invalid: Narrow = @bitCast(Metadata{ .fingerprint = 1 });
        pub fn read() Key { return invalid; }
    };
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return @intCast(Table(u16).read()); }

const Options = struct {
    fn implementation(value: u16) u8 { return @truncate(value); }
    pub const typed: fn (u8) u8 = implementation;
};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return Options.typed(42); }

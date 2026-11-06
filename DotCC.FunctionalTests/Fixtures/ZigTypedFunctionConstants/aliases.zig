const provider = @import("provider.zig");
const relay = @import("relay.zig");
const Local = struct {
    fn implementation(value: u8) u8 { return value + 1; }
    pub const typed: fn (u8) u8 = implementation;
};
fn caller(comptime T: type) u8 {
    const value: T = 42;
    if (provider.forwarded(@intCast(value)) != .answer) return 1;
    if (provider.Options.typed(42) != .answer) return 2;
    if (relay.forwarded(42) != .answer) return 3;
    return Local.typed(41);
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return caller(u64); }

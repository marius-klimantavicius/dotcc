var calls: u8 = 0;
fn next() u16 { calls += 1; return 42; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var small: [4097]u8 = @splat(0);
    small[4096] = 7;
    for (small[0..4096]) |byte| if (byte != 0) return 1;
    if (small[4096] != 7) return 2;
    var wide: [8193]u16 = @splat(next());
    if (calls != 1) return 3;
    wide[8192] = 9;
    for (wide[0..8192]) |value| if (value != 42) return 4;
    if (wide[8192] != 9 or calls != 1) return 5;
    return 42;
}

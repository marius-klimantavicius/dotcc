var calls: u8 = 0;
fn read(present: bool) ?u32 {
    calls += 1;
    return if (present) 42 else null;
}
pub fn probe(present: bool) callconv(.c) u32 { return read(present) orelse unreachable; }
pub fn seen() callconv(.c) u8 { return calls; }
comptime {
    @export(&answer, .{ .name = "answer" });
    @export(&probe, .{ .name = "probe" });
    @export(&seen, .{ .name = "seen" });
}
pub fn answer() callconv(.c) u8 {
    if (probe(true) != 42 or calls != 1) return 1;
    return 42;
}

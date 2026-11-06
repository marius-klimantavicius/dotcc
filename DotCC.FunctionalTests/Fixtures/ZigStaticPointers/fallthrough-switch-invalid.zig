const Mode = enum { good, falls_through };
fn select(mode: Mode) u8 {
    switch (mode) {
        .good => return 42,
        .falls_through => {},
    }
    comptime unreachable;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return select(.good); }

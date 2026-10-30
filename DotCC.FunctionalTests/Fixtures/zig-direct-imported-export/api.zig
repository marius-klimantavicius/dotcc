pub const marker: u8 = 1;

fn answer() callconv(.c) c_int {
    return 42;
}

comptime {
    @export(&answer, .{ .name = "zig_direct_imported_answer" });
}

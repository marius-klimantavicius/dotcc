fn answer() callconv(.c) c_int {
    return 42;
}

comptime {
    @export(&answer, .{ .name = "zig_direct_answer" });
    @export(&answer, .{ .name = "zig_direct_answer_alias" });
}

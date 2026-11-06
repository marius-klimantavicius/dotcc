fn change(value: [*:0]u8) void { value[0] = 9; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var storage: [2:0]u8 = .{ 1, 2 };
    const readonly: [:0]const u8 = &storage;
    change(readonly);
    return 42;
}

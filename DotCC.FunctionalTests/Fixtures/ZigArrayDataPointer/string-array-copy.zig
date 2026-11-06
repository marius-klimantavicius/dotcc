comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var owned: [4]u8 = "AB\x00D".*;
    owned[0] = 'X';
    if (owned.len != 4 or owned[0] != 'X' or owned[2] != 0 or owned[3] != 'D') return 1;
    if ("AB\x00D"[0] != 'A') return 2;
    var terminated: [3:0]u8 = "ABC".*;
    terminated[1] = 'Z';
    if (terminated[1] != 'Z') return 3;
    const pointer: [*:0]const u8 = &terminated;
    if (pointer[3] != 0) return 4;
    return 42;
}

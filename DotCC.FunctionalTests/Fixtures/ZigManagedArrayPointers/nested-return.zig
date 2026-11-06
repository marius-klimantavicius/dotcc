const Cell = extern struct { value: u32 };
fn grid(seed: u32) [2][3]Cell {
    var result: [2][3]Cell = undefined;
    for (0..2) |i| {
        for (0..3) |j| result[i][j] = .{ .value = seed + @as(u32, @intCast(i * 3 + j)) };
    }
    return result;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var first = grid(37);
    var second = grid(50);
    if (first[0][0].value != 37 or first[1][2].value != 42 or second[1][2].value != 55) return 1;
    if (@intFromPtr(&first[1][0]) - @intFromPtr(&first[0][0]) != 3 * @sizeOf(Cell)) return 2;
    if (@intFromPtr(&first[0][0]) == @intFromPtr(&second[0][0])) return 3;
    first[0][0].value = 99;
    second[1][2].value = 70;
    if (first[1][2].value != 42 or second[0][0].value != 50) return 4;
    return @intCast(first[1][2].value);
}

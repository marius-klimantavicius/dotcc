var calls: u32 = 0;
fn entry(values: []?[2]u16, index: usize) *?[2]u16 {
    calls += 1;
    return &values[index];
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var values = [_]?[2]u16{ .{ 17, 25 }, null };
    const view: []?[2]u16 = &values;
    const pair: *[2]u16 = &entry(view, 0).*.?;
    pair.*[0] = 19;
    if (view[0]) |*captured| captured.*[1] = 23;
    if (calls != 1 or values[0].?[0] + values[0].?[1] != 42) return 1;
    if (values[1] != null) return 2;
    return 42;
}

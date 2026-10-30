fn mutate(values: []*u8) void { values[0].* += values[1].*; }
fn read(values: []const *u8) u8 { return values[0].* + values[1].*; }
pub fn main() u8 {
    var first: u8 = 20;
    var second: u8 = 7;
    var pointers = [_]*u8{ &first, &second };
    const view: []*u8 = &pointers;
    mutate(view);
    const readonly: []const *u8 = view;
    if (readonly[0] != &first or readonly[1] != &second) return 1;
    return read(readonly) + @as(u8, @intCast(view.len));
}

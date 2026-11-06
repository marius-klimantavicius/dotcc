var conditions: usize = 0;
var values: usize = 0;
fn condition(flag: bool) bool { conditions += 1; return flag; }
fn amount(index: usize, start: usize) usize { values += 1; return index - start + 1; }
fn choose(flag: bool, index: usize, start: usize) usize {
    const value = if (condition(flag)) 0 else amount(index, start);
    if (@TypeOf(value) != usize) @compileError("leading literal lost its usize peer");
    return value;
}
fn reverse(flag: bool, index: usize, start: usize) usize {
    const value = if (condition(flag)) amount(index, start) else 0;
    if (@TypeOf(value) != usize) @compileError("trailing literal lost its usize peer");
    return value;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (choose(true, 5, 5) != 0 or conditions != 1 or values != 0) return 1;
    if (choose(false, 7, 5) != 3 or conditions != 2 or values != 1) return 2;
    if (reverse(false, 7, 5) != 0 or conditions != 3 or values != 1) return 3;
    if (reverse(true, 7, 5) != 3 or conditions != 4 or values != 2) return 4;
    return 42;
}

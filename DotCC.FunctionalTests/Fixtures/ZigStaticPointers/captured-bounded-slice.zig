var order: u32 = 0;
fn condition(ok: bool) error{Failed}!void {
    order = order * 10 + 1;
    if (!ok) return error.Failed;
}
fn success(buffer: []u8) []u8 { order = order * 10 + 2; return buffer[0..2]; }
fn run(ok: bool) u8 {
    order = 0;
    var buffer: [4]u8 = .{ 10, 20, 30, 40 };
    const msg = if (condition(ok)) |_| success(&buffer) else |_| blk: {
        order = order * 10 + 3;
        buffer[3] = 42;
        break :blk &buffer;
    };
    if (ok) {
        if (msg.len != 2 or order != 12 or msg[1] != 20) return 1;
    } else {
        if (msg.len != 4 or order != 13 or msg[3] != 42) return 2;
    }
    msg[0] = 11;
    return if (buffer[0] == 11) 42 else 3;
}
fn readOnly(ok: bool) u8 {
    var mutable: [2]u8 = .{ 10, 20 };
    const fixed: [3]u8 = .{ 31, 32, 33 };
    const msg = if (condition(ok)) |_| mutable[0..] else |_| blk: { break :blk &fixed; };
    if (!@typeInfo(@TypeOf(msg)).pointer.attrs.@"const") return 1;
    return if (msg.len == 3 and msg[2] == 33) 42 else 2;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (run(true) != 42 or run(false) != 42 or readOnly(false) != 42) return 1;
    return 42;
}

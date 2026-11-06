const enabled = false;
const Command = union(enum) { absent: void, byte: u8 };
var effects: u8 = 0;
fn visit(command: *Command) void {
    effects += 1;
    switch (command.*) {
        .absent => |*payload| if (comptime enabled) {
            _ = payload;
            effects = 99;
        },
        .byte => |*payload| { payload.* += 1; },
    }
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    effects = 0;
    var a: Command = .{ .absent = {} };
    var b: Command = .{ .byte = 39 };
    visit(&a);
    visit(&b);
    if (effects != 2 or b.byte != 40) return 1;
    return 42;
}

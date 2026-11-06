const Command = union(enum) { kitty: u8, other: u8 };
var calls: u8 = 0;
fn amount(value: ?u8) ?u8 { calls += 1; return value; }
fn handle(command: *Command, value: ?u8) u8 {
    var score: u8 = 1;
    switch (command.*) {
        .kitty => |*payload| kitty: {
            defer score += 10;
            const n = amount(value) orelse break :kitty;
            payload.* += n;
            score += 2;
        },
        .other => |*payload| other: {
            const n = amount(value) orelse break :other;
            payload.* += n;
            score += 3;
        },
    }
    return score;
}
pub fn main() u8 {
    calls = 0;
    var command: Command = .{ .kitty = 4 };
    if (handle(&command, null) != 11 or command.kitty != 4) return 1;
    if (handle(&command, 2) != 13 or command.kitty != 6) return 2;
    command = .{ .other = 7 };
    if (handle(&command, null) != 1 or command.other != 7) return 3;
    if (handle(&command, 3) != 4 or command.other != 10 or calls != 4) return 4;
    return 42;
}

const Parser = @import("vt").Parser;
const Action = Parser.Action;

fn sum(actions: []const ?Action) u32 {
    var result: u32 = 0;
    for (actions) |optional| {
        const action = optional orelse continue;
        switch (action) {
            .print => |value| result += value,
            .execute => |value| result += value,
            .dcs_unhook => result += 1,
            else => {},
        }
    }
    return result;
}

fn fill(actions: []?Action) void {
    @memset(actions, null);
    actions[0] = .{ .print = 20 };
    actions[1] = .{ .execute = 21 };
    actions[2] = .{ .dcs_unhook = {} };
}

comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var source: [5]?Action = @splat(null);
    fill(source[1..4]);
    if (source[0] != null or source[4] != null or sum(&source) != 42) return 1;
    var copy = source;
    const view: []?Action = copy[1..4];
    if (@intFromPtr(view.ptr) != @intFromPtr(&copy[1]) or view.len != 3) return 2;
    if (@intFromPtr(&view[1]) - @intFromPtr(&view[0]) != @sizeOf(?Action)) return 3;
    view[0] = null;
    if (copy[1] != null or source[1] == null or sum(&copy) != 22) return 4;
    if (view[1]) |*payload| payload.* = .{ .execute = 40 };
    view[0] = .{ .print = 1 };
    if (sum(&copy) != 42 or sum(&source) != 42) return 5;
    const readonly: []const ?Action = view;
    const mutable = @constCast(readonly);
    mutable[0] = .{ .print = 2 };
    if (sum(&copy) != 43 or sum(&source) != 42) return 6;
    mutable[0] = .{ .print = 1 };
    @memcpy(source[1..4], view);
    @memcpy(view.ptr, source[1..4]);
    @memcpy(source[1..4], view.ptr);
    if (sum(&source) != 42 or sum(&copy) != 42) return 7;
    @memmove(copy[2..5], copy[1..4]);
    if (sum(&copy) != 43) return 8;
    @memmove(copy[1..4], copy[2..5]);
    copy[4] = null;
    if (sum(&copy) != 42) return 9;
    @memset(view, null);
    if (sum(&copy) != 0 or sum(&source) != 42) return 10;
    @memcpy(view, source[1..4]);
    if (sum(view[0..2]) != 41 or sum(&copy) != 42) return 11;

    return 42;
}

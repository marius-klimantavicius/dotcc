const std = @import("std");

fn diagnose(comptime fmt: []const u8, args: anytype) u32 {
    if ((@inComptime())) {
        @compileError(std.fmt.comptimePrint(fmt, args));
    } else {
        return args[0] + 40;
    }
}

fn inverse(value: u32) u32 {
    if ((!@inComptime())) {
        return value;
    } else {
        @compileError("inverse is runtime only");
    }
}

fn fold(comptime fail: bool) u32 {
    if (@inComptime()) {
        if (fail) @compileError("selected failure");
        return 2;
    } else {
        return 100;
    }
}

comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u32 {
    var value: u32 = comptime fold(false);
    value += 0;
    return inverse(diagnose("rejected value {d}", .{value}));
}

pub fn main() u8 { return @intCast(answer()); }

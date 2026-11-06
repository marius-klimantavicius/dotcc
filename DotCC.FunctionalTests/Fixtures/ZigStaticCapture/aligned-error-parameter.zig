fn inspect(timeout: u128, output: *u128, fail: bool) error{Expired}!void {
    if (@intFromPtr(&timeout) & 15 != 0) return error.Expired;
    output.* = timeout +% 1;
    if (fail) return error.Expired;
}

comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var result: u128 = 0;
    inspect(18446744073709551657, &result, false) catch return 1;
    if (result != 18446744073709551658) return 2;
    var caught = false;
    inspect(340282366920938463463374607431768211455, &result, true) catch {
        caught = true;
    };
    if (!caught or result != 0) return 3;
    inspect(41, &result, false) catch return 4;
    return @intCast(result);
}

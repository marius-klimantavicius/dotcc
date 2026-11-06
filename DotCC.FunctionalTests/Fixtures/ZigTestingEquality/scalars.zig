const std = @import("std");
const Width = enum(u8) { narrow = 1, wide = 2 };

fn next(counter: *u32) u8 {
    counter.* += 1;
    return @intCast(counter.*);
}
fn ordered(counter: *u32, marker: u32) u8 {
    counter.* = counter.* * 10 + marker;
    return 42;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const narrow: Width = .narrow;
    const wide: Width = .wide;
    std.testing.expectEqual(narrow, narrow) catch return 1;
    var caught: u32 = 0;
    std.testing.expectEqual(narrow, wide) catch { caught += 1; };
    const nan: f32 = @bitCast(@as(u32, 0x7fc00000));
    std.testing.expectEqual(nan, nan) catch { caught += 1; };
    const nan64: f64 = @bitCast(@as(u64, 0x7ff8000000000000));
    std.testing.expectEqual(nan64, nan64) catch { caught += 1; };
    std.testing.expectEqual(@as(f32, 0.0), -@as(f32, 0.0)) catch return 2;
    std.testing.expectEqual(@as(f64, 0.0), -@as(f64, 0.0)) catch return 3;
    var counter: u32 = 0;
    std.testing.expectEqual(next(&counter), next(&counter)) catch { caught += 1; };
    if (counter != 2 or caught != 4) return 4;
    counter = 0;
    std.testing.expectEqual(ordered(&counter, 1), ordered(&counter, 2)) catch return 5;
    if (counter != 12) return 6;
    return 42;
}

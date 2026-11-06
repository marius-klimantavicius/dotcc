const Source = @import("ComptimeSource.zig");
const first: Source = .{ .value = 40 };
const second: Source = .{ .value = 41 };
fn counted(calls: *u8) u8 { calls.* += 1; return 2; }
pub fn main() u8 {
    var calls: u8 = 0;
    if (first.add(counted(&calls)) != 42 or calls != 1) return 1;
    if (second.add(1) != 42 or first.add(0) != 40) return 2;
    const known = comptime first.constant();
    if (known != 40) return 3;
    var runtime: Source = .{ .value = 39 };
    runtime.value += calls;
    return runtime.addRuntime(2);
}

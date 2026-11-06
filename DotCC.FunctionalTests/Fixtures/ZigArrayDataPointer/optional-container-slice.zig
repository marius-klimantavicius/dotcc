const std = @import("std");
fn count(value: anytype) usize { return std.mem.absorbSentinel(value).len; }
const Plain = struct {
    var last_data: ?[]u8 = null;
    fn check() u8 {
        if (last_data) |d| {
            if (@typeInfo(@TypeOf(d)).pointer.sentinel_ptr != null) return 1;
            const raw = std.mem.absorbSentinel(d);
            if (raw.ptr != d.ptr or raw.len != 3 or raw[2] != 3) return 2;
            return 42;
        }
        return 3;
    }
};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const Terminated = struct {
        var last_data: ?[:0]u8 = null;
        fn check() u8 {
            if (last_data) |d| {
                if (@typeInfo(@TypeOf(d)).pointer.sentinel_ptr == null) return 4;
                const raw = std.mem.absorbSentinel(d);
                if (raw.ptr != d.ptr or raw.len != 4 or raw[3] != 0) return 5;
                return 42;
            }
            return 6;
        }
    };
    var bytes: [3:0]u8 = .{ 1, 2, 3 };
    if (Plain.check() != 3 or Terminated.check() != 6) return 7;
    Plain.last_data = bytes[0..3];
    const terminated: [:0]u8 = &bytes;
    Terminated.last_data = terminated;
    if (Plain.check() != 42 or Terminated.check() != 42) return 8;
    if (count(Plain.last_data.?) != 3 or count(Terminated.last_data.?) != 4) return 10;
    Plain.last_data = null;
    Terminated.last_data = null;
    if (Plain.check() != 3 or Terminated.check() != 6) return 9;
    return 42;
}

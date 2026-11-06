const std = @import("std");
const Report = @import("modes").Report;
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var max_written: usize = 0;
    const ids = [_]u16{ 0, 9, 10, 99, 100, 999, 1000, 9999, 10000, 65535 };
    for (ids) |id| {
        for ([_]bool{ false, true }) |ansi| {
            for ([_]u8{ 0, 1, 2, 3, 4 }) |state| {
                const report: Report = .{ .tag = .{ .value = id, .ansi = ansi }, .state = @fromBackingInt(state) };
                var buffer: [Report.max_size + 2]u8 = @splat(0xA5);
                var writer = std.Io.Writer.fixed(buffer[1 .. buffer.len - 1]);
                report.encode(&writer) catch return 1;
                var expected: [Report.max_size]u8 = undefined;
                var end: usize = 0;
                expected[end] = 0x1B; end += 1;
                expected[end] = '['; end += 1;
                if (!ansi) { expected[end] = '?'; end += 1; }
                var digits: [5]u8 = undefined;
                var offset: usize = digits.len;
                var remaining = id;
                while (true) {
                    offset -= 1;
                    digits[offset] = @intCast('0' + remaining % 10);
                    remaining /= 10;
                    if (remaining == 0) break;
                }
                for (digits[offset..]) |digit| { expected[end] = digit; end += 1; }
                expected[end] = ';'; end += 1;
                expected[end] = '0' + state; end += 1;
                expected[end] = '$'; end += 1;
                expected[end] = 'y'; end += 1;
                if (!std.mem.eql(u8, writer.buffered(), expected[0..end])) return 2;
                max_written = @max(max_written, end);
                if (buffer[0] != 0xA5 or buffer[buffer.len - 1] != 0xA5) return 3;
                var short: [Report.max_size + 2]u8 = @splat(0xA5);
                var short_writer = std.Io.Writer.fixed(short[1..end]);
                const failed = if (report.encode(&short_writer)) |_| false else |err| err == error.WriteFailed;
                if (!failed or short[0] != 0xA5 or short[end] != 0xA5) return 4;
            }
        }
    }
    if (max_written != Report.max_size) return 5;
    return 42;
}

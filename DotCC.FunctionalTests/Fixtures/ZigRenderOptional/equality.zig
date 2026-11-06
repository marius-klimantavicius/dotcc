const std = @import("std");
// These checks intentionally produce two assertion diagnostics. Retain and
// inspect them through a buffered provider; Io.failing's stderr hooks trap.
pub const std_options_debug_io: std.Io = .{ .userdata = null, .vtable = &debug_vtable };
const debug_vtable = blk: {
    var table = std.Io.failing.vtable.*;
    table.swapCancelProtection = swapCancelProtection;
    table.lockStderr = lockStderr;
    table.unlockStderr = unlockStderr;
    break :blk table;
};
var protection: std.Io.CancelProtection = .unblocked;
var stderr_writer: std.Io.File.Writer = undefined;
var diagnostics: [128]u8 = undefined;
var diagnostic_len: usize = 0;
var diagnostic_count: u32 = 0;
fn swapCancelProtection(_: ?*anyopaque, next: std.Io.CancelProtection) std.Io.CancelProtection {
    const previous = protection;
    protection = next;
    return previous;
}
fn lockStderr(_: ?*anyopaque, _: ?std.Io.Terminal.Mode) std.Io.Cancelable!std.Io.LockedStderr {
    stderr_writer = std.Io.File.stderr().writerStreaming(std_options_debug_io, &.{});
    return .{ .file_writer = &stderr_writer, .terminal_mode = .no_color };
}
fn unlockStderr(_: ?*anyopaque) void {
    const written = stderr_writer.interface.buffer[0..stderr_writer.interface.end];
    @memcpy(diagnostics[diagnostic_len..][0..written.len], written);
    diagnostic_len += written.len;
    diagnostic_count += 1;
    stderr_writer.interface.end = 0;
}
const Viewport = struct { x: u16, y: u32 };
var order: u32 = 0;
fn value(marker: u32) ?u64 { order = order * 10 + marker; return 42; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    std.testing.expectEqual(value(1), value(2)) catch return 1;
    if (order != 12) return 2;
    std.testing.expectEqual(@as(?u64, null), @as(?u64, null)) catch return 3;
    const left: ?Viewport = .{ .x = 17, .y = 25 };
    const right: ?Viewport = .{ .x = 17, .y = 25 };
    std.testing.expectEqual(left, right) catch return 4;
    var caught: u32 = 0;
    std.testing.expectEqual(@as(?u64, 17), @as(?u64, 18)) catch { caught += 1; };
    const nan: f64 = @bitCast(@as(u64, 0x7ff8000000000000));
    std.testing.expectEqual(@as(?f64, nan), @as(?f64, nan)) catch { caught += 1; };
    if (caught != 2) return 5;
    if (diagnostic_count != 2 or protection != .unblocked) return 6;
    if (!std.mem.eql(u8, diagnostics[0..diagnostic_len], "expected 17, found 18\nexpected nan, found nan\n")) return 7;
    return 42;
}

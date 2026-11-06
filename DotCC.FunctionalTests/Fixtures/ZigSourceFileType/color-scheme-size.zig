const std = @import("std");
const ColorScheme = enum { light, dark };
const max_size = max: {
    var result: usize = 0;
    for (@typeInfo(ColorScheme).@"enum".field_values) |field_value| {
        result = @max(result, colorSchemeReportBytes(@fromBackingInt(field_value)).len);
    }
    break :max result;
};
fn colorSchemeReportBytes(scheme: ColorScheme) []const u8 {
    return switch (scheme) { .dark => "\x1B[?997;1n", .light => "\x1B[?997;2n" };
}
fn encode(writer: *std.Io.Writer, scheme: ColorScheme) std.Io.Writer.Error!void {
    try writer.writeAll(colorSchemeReportBytes(scheme));
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (max_size != 9) return 1;
    inline for (.{ ColorScheme.light, ColorScheme.dark }) |scheme| {
        var buffer: [max_size + 2]u8 = @splat(0xA5);
        var writer = std.Io.Writer.fixed(buffer[1 .. buffer.len - 1]);
        encode(&writer, scheme) catch return 2;
        const expected: []const u8 = if (scheme == .dark) "\x1B[?997;1n" else "\x1B[?997;2n";
        if (!std.mem.eql(u8, writer.buffered(), expected)) return 3;
        if (buffer[0] != 0xA5 or buffer[buffer.len - 1] != 0xA5) return 4;
        var short: [max_size - 1]u8 = undefined;
        var short_writer = std.Io.Writer.fixed(&short);
        const failed = if (encode(&short_writer, scheme)) |_| false else |err| err == error.WriteFailed;
        if (!failed) return 5;
    }
    return 42;
}

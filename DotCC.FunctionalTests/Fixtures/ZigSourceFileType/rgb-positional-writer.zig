const std = @import("std");
const RGB = packed struct(u24) { r: u8 = 0, g: u8 = 0, b: u8 = 0 };

fn encode(writer: *std.Io.Writer, rgb: RGB, comptime background: bool, comptime named: bool) std.Io.Writer.Error!void {
    if (background) {
        if (named) try writer.print(";48:2::{[r]}:{[g]}:{[b]}", rgb)
        else try writer.print(";48:2::{}:{}:{}", .{ rgb.r, rgb.g, rgb.b });
    } else {
        if (named) try writer.print(";38:2::{[r]}:{[g]}:{[b]}", rgb)
        else try writer.print(";38:2::{}:{}:{}", .{ rgb.r, rgb.g, rgb.b });
    }
}

comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var buffer: [80]u8 = undefined;
    var writer: std.Io.Writer = .fixed(&buffer);
    encode(&writer, .{ .r = 0, .g = 128, .b = 255 }, false, false) catch return 1;
    encode(&writer, .{ .r = 255, .g = 1, .b = 7 }, true, false) catch return 2;
    const actual = writer.buffered();
    const expected = ";38:2::0:128:255;48:2::255:1:7";
    if (actual.len != expected.len) return 3;
    for (actual, expected) |a, b| { if (a != b) return 4; }
    var short: [8]u8 = undefined;
    var short_writer: std.Io.Writer = .fixed(&short);
    encode(&short_writer, .{ .r = 9, .g = 10, .b = 11 }, false, false) catch |err| {
        if (err != error.WriteFailed) return 5;
        const partial = short_writer.buffered();
        if (partial.len != 8) return 6;
        for (partial, ";38:2::9") |a, b| { if (a != b) return 7; }
        return 42;
    };
    return 8;
}

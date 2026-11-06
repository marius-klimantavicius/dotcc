const std = @import("std");
const encoding = @import("encoding");
fn check(input: []const u8, expected: []const u8) bool {
    var buffer: [64]u8 = undefined;
    var writer: std.Io.Writer = .fixed(&buffer);
    encoding.printfQDecode(&writer, input) catch return false;
    const actual = writer.buffered();
    if (actual.len != expected.len) return false;
    for (actual, expected) |a, b| { if (a != b) return false; }
    return true;
}
fn rejected(input: []const u8) bool {
    var buffer: [64]u8 = undefined;
    var writer: std.Io.Writer = .fixed(&buffer);
    encoding.printfQDecode(&writer, input) catch |err| return err == error.DecodeError;
    return false;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (!check("plain", "plain") or !check("'quoted'", "quoted")) return 1;
    if (!check("a\\ b\\\\c\\\"d\\'e\\$f", "a b\\c\"d'e$f")) return 2;
    if (!check("$'x\\e\\n\\r\\t\\vy'", "x\x1b\n\r\t\x0by")) return 3;
    if (!check("''", "") or !check("$''", "")) return 4;
    if (!rejected("\\") or !rejected("\\q") or !rejected("'unclosed") or !rejected("$'unclosed")) return 5;
    var short: [1]u8 = undefined;
    var writer: std.Io.Writer = .fixed(&short);
    encoding.printfQDecode(&writer, "abc") catch |err| { return if (err == error.WriteFailed) 42 else 6; };
    return 7;
}

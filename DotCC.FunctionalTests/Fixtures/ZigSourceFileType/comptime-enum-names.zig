const std = @import("std");
fn names(comptime E: type, comptime skip: []const E) []const u8 {
    comptime {
        var result: []const u8 = "";
        for (@typeInfo(E).@"enum".field_names) |field| {
            const value = @field(E, field);
            if (std.mem.indexOfScalar(E, skip, value) != null) continue;
            if (result.len > 0) result = result ++ ",";
            result = result ++ field;
        }
        return result;
    }
}
const State = enum { clear, idle, working, @"needs-help" };
const Kind = enum { permission, question, auth };
const Empty = enum {};
const reply = "?:states=" ++ names(State, &.{.clear}) ++ ":kinds=" ++ names(Kind, &.{});
const trimmed = names(State, &.{ .clear, .idle });
const removed = names(State, &.{ .clear, .idle, .working, .@"needs-help" });
const empty = names(Empty, &.{});
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (!std.mem.eql(u8, reply, "?:states=idle,working,needs-help:kinds=permission,question,auth")) return 1;
    if (!std.mem.eql(u8, trimmed, "working,needs-help")) return 2;
    if (!std.mem.eql(u8, removed, "")) return 3;
    if (!std.mem.eql(u8, empty, "")) return 4;
    return 42;
}

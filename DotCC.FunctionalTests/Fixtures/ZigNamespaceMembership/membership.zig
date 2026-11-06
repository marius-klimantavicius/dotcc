const std = @import("std");
const declarations = @import("declarations.zig");
fn contains(comptime Namespace: type, comptime name: []const u8) bool { return @hasDecl(Namespace, name); }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (!@hasDecl(std, "StringHashMap") or !contains(std, "StringHashMap")) return 1;
    if (@hasDecl(std, "dotcc_definitely_missing")) return 2;
    if (!contains(declarations, "Public") or !contains(declarations, "function")) return 3;
    if (contains(declarations, "Private") or contains(declarations, "privateFunction")) return 4;
    if (contains(declarations, "missing") or declarations.own()) return 5;
    return 42;
}

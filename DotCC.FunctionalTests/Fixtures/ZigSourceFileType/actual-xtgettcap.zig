const Source = @import("source");
const source: Source = .{ .names = &.{"probe"}, .capabilities = &.{
    .{ .name = "am", .value = .{ .boolean = {} } },
    .{ .name = "zero", .value = .{ .numeric = 0 } },
    .{ .name = "max", .value = .{ .numeric = 4294967295 } },
    .{ .name = "kx", .value = .{ .string = "^?" } },
    .{ .name = "kbs", .value = .{ .string = "^H" } },
    .{ .name = "kf1", .value = .{ .string = "\\EOP" } },
    .{ .name = "Smulx", .value = .{ .string = "\\E[4:%p1%dm" } },
    .{ .name = "empty", .value = .{ .string = "" } },
} };
const map = source.xtgettcapMap();
fn same(value: [:0]const u8, expected: []const u8) bool {
    if (value.len != expected.len or value.ptr[value.len] != 0) return false;
    for (value, expected) |actual, wanted| if (actual != wanted) return false;
    return true;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (map.keys().len != 11 or map.get("missing") != null) return 1;
    if (!same(map.get("544E").?, "\x1bP1+r544E=70726F6265\x1b\\")) return 2;
    if (!same(map.get("436F").?, "\x1bP1+r436F=323536\x1b\\")) return 3;
    if (!same(map.get("524742").?, "\x1bP1+r524742=38\x1b\\")) return 4;
    if (!same(map.get("616D").?, "\x1bP1+r616D\x1b\\")) return 5;
    if (!same(map.get("7A65726F").?, "\x1bP1+r7A65726F=30\x1b\\")) return 6;
    if (!same(map.get("6D6178").?, "\x1bP1+r6D6178=34323934393637323935\x1b\\")) return 7;
    if (!same(map.get("6B78").?, "\x1bP1+r6B78=7F\x1b\\")) return 8;
    if (!same(map.get("6B6273").?, "\x1bP1+r6B6273=08\x1b\\")) return 9;
    if (!same(map.get("6B6631").?, "\x1bP1+r6B6631=1B4F50\x1b\\")) return 10;
    if (!same(map.get("536D756C78").?, "\x1bP1+r536D756C78=5C455B343A25703125646D\x1b\\")) return 11;
    if (!same(map.get("656D707479").?, "\x1bP1+r656D707479\x1b\\")) return 12;
    return 42;
}

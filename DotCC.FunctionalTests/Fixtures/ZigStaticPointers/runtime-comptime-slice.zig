fn Table(comptime keys: []const u16) type {
    return struct {
        pub fn len() usize { return keys.len; }
    };
}
fn demand(keys: []const u16) u8 {
    const copied = keys;
    const Runtime = Table(copied);
    return @intCast(Runtime.len());
}
var values: [2]u16 = .{ 17, 42 };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    return demand(&values);
}

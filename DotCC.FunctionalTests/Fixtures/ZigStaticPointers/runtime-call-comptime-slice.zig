fn Table(comptime keys: []const u16) type {
    return struct {
        pub fn len() usize { return keys.len; }
    };
}
fn supply() []const u16 { return &.{ 17, 42 }; }
fn demand() u8 {
    const keys = supply();
    const Runtime = Table(keys);
    return @intCast(Runtime.len());
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    return demand();
}

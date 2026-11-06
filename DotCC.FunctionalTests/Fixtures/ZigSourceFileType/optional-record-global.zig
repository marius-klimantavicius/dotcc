const Version = struct { major: u32, minor: u32 };
const Row = struct { version: ?Version };
const rows = [_]Row{ .{ .version = .{ .major = 40, .minor = 2 } }, .{ .version = null } };
pub fn main() u8 {
    const first = rows[0].version orelse return 1;
    if (rows[1].version != null) return 2;
    return @intCast(first.major + first.minor);
}

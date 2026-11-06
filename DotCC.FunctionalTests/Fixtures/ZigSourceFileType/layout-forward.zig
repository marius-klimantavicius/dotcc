const Wrapper = struct { pool: Pool(Later) };
const Pool = @import("layout-pool.zig").Pool;
const Later = extern struct { wide: u64, narrow: u8 };
pub fn main() u8 {
    if (@sizeOf(Later) != 16 or @alignOf(Later) != 8 or @sizeOf(Wrapper) != 8) return 1;
    var payload: Later = .{ .wide = 40, .narrow = 2 };
    const wrapper: Wrapper = .{ .pool = .{ .ptr = &payload } };
    return @intCast(wrapper.pool.ptr.wide + wrapper.pool.ptr.narrow);
}

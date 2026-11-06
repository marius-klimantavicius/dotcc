const Options = struct {
    geometry: ?struct { width: u32, height: u32 } = null,
};
fn read(options: Options) u32 {
    if (options.geometry) |geometry| return geometry.width + geometry.height;
    return 0;
}
pub fn main() u8 {
    if (read(.{}) != 0) return 1;
    if (read(.{ .geometry = .{ .width = 17, .height = 25 } }) != 42) return 2;
    return 42;
}

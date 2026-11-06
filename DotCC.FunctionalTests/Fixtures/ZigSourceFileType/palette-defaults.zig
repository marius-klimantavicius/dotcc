const palette = @This();
pub const RGB = packed struct(u24) { r: u8, g: u8, b: u8 };
pub const Palette = [4]RGB;
pub const default: Palette = .{
    .{ .r = 1, .g = 2, .b = 3 },
    .{ .r = 11, .g = 12, .b = 13 },
    .{ .r = 21, .g = 22, .b = 23 },
    .{ .r = 31, .g = 32, .b = 33 },
};
pub const Dynamic = struct {
    current: Palette,
    original: *const Palette,
    mask: u8,
    pub const default: Dynamic = .{ .current = palette.default, .original = &palette.default, .mask = 0 };
};

const RGB = packed struct(u24) {
    r: u8, g: u8, b: u8,
    const C = extern struct { r: u8, g: u8, b: u8 };
    pub fn fromC(c: C, count: *u8) RGB {
        count.* += 1;
        return .{ .r = c.r, .g = c.g, .b = c.b };
    }
};
fn convert(c: ?*const RGB.C, count: *u8) ?RGB {
    return if (c) |v| .fromC(v.*, count) else null;
}
fn direct(c: RGB.C, count: *u8) ?RGB { return .fromC(c, count); }
pub fn main() u8 {
    var count: u8 = 0;
    const c: RGB.C = .{ .r = 1, .g = 42, .b = 3 };
    var color: ?RGB = null;
    color = if (@as(?*const RGB.C, &c)) |v| .fromC(v.*, &count) else null;
    if (color.?.g != 42 or count != 1) return 1;
    if (convert(null, &count) != null or count != 1) return 2;
    const converted = convert(&c, &count);
    if (converted.?.r != 1 or count != 2) return 3;
    const other = direct(c, &count);
    if (other.?.b != 3 or count != 3) return 4;
    return color.?.g;
}

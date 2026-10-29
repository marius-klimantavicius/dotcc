const facade = @import("import-type-alias-facade.zig");
const Point = facade.geometry.Point;
const Scalar = facade.Scalar;

pub fn main() u8 {
    const point: Point = .{ .x = 20, .y = 22 };
    const sum: Scalar = point.x + point.y;
    return @intCast(sum);
}

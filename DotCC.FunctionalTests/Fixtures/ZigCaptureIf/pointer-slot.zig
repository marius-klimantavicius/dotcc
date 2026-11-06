const Box = struct { ptr: ?*u32 };
var calls: u32 = 0;
fn getBox(box: *Box) *Box { calls += 1; return box; }
fn replace(box: *Box, replacement: *u32) void {
    const slot = if (getBox(box).ptr) |*p| p else { return; };
    slot.* = replacement;
}
pub fn main() u8 {
    var a: u32 = 1;
    var b: u32 = 42;
    var box: Box = .{ .ptr = &a };
    replace(&box, &b);
    if (box.ptr.? != &b or box.ptr.?.* != 42 or calls != 1) return 1;
    box.ptr = null;
    replace(&box, &a);
    if (box.ptr != null or calls != 2) return 2;
    return 42;
}

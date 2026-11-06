const forward = @import("forward.zig");
const contexts = @import("contexts.zig");
pub fn main() u8 {
    const A = contexts.Box(u8, 2).Context;
    const B = contexts.Box(u16, 3).Context;
    if (A == B) return 1;
    if (A != contexts.Box(u8, 2).Context) return 2;
    if (contexts.First == contexts.Second) return 3;
    if (A.width != 8 or B.width != 16) return 4;
    var a: A = .{ .value = 10, .inner = .{ .value = 8 }, .bytes = undefined };
    var b: B = .{ .value = 11, .inner = .{ .value = 8 }, .bytes = undefined };
    var v: forward.Set = .{ .value = .{ .value = 0 } };
    v.value.value = a.answer() + b.answer();
    return v.value.value;
}

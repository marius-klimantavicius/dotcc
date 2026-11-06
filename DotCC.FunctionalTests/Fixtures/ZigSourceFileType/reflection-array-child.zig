const std = @import("std");
fn plain(comptime P: type) bool {
    const child = @typeInfo(P).pointer.child;
    return @typeInfo(child).array.sentinel_ptr == null;
}
const Plain = std.meta.Slice(*[16]u8);
const Sent = std.meta.Slice(*[16:7]u8);
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (Plain != []u8) return 1;
    if (Sent != [:7]u8) return 2;
    if (!plain(*[16]u8)) return 3;
    if (plain(*[16:7]u8)) return 4;
    var bytes: [16]u8 = @splat(3);
    const slice: Plain = &bytes;
    if (slice.len != 16 or slice[15] != 3) return 5;
    var sent: [16:7]u8 = @splat(2);
    const sentinel_slice: Sent = &sent;
    if (sentinel_slice.len != 16 or sentinel_slice[16] != 7) return 6;
    return 42;
}

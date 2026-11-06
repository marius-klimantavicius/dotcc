const Packet = extern struct { a: u64, b: u64, c: u64, d: u64, e: u64, f: u64, g: u64 };
const Callback = *const fn (Packet, ?*anyopaque) callconv(.c) Packet;
const Flag = *const fn (bool) callconv(.c) bool;
var calls: u8 = 0;
fn transform(value: Packet, context: ?*anyopaque) callconv(.c) Packet {
    if (context != null) unreachable;
    calls += 1;
    var result = value;
    result.g = value.a + value.b + value.c + value.d + value.e + value.f + value.g;
    return result;
}
fn flip(value: bool) callconv(.c) bool { return !value; }
fn dispatch(value: Packet) callconv(.c) Packet {
    const callback: Callback = &transform;
    return callback(value, null);
}
comptime { @export(&dispatch, .{ .name = "managed_packet_dispatch" }); }
pub fn main() u8 {
    const flag: Flag = &flip;
    if (!flag(false) or flag(true)) return 1;
    if (@sizeOf(Packet) != 56) return 2;
    const packet = dispatch(.{ .a = 1, .b = 2, .c = 3, .d = 4, .e = 5, .f = 6, .g = 21 });
    if (calls != 1 or packet.a != 1 or packet.f != 6) return 3;
    return @intCast(packet.g);
}

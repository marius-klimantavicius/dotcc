const Payload = struct { value: u64 };
var calls: u8 = 0;
fn left() ?Payload { calls += 1; return .{ .value = 17 }; }
fn right() ?Payload { calls += 1; return null; }
fn payload() Payload { calls += 1; return .{ .value = 42 }; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const Peer = @TypeOf(left(), right());
    if (Peer != ?Payload or calls != 0) return 1;
    if (@TypeOf(payload(), right()) != ?Payload or @TypeOf(left(), payload()) != ?Payload or calls != 0) return 2;
    const value: Peer = .{ .value = 42 };
    return @intCast(value.?.value);
}

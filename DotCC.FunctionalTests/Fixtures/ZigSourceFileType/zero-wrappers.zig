const Empty = struct {};
const Io = struct {
    impl: Empty,
    fn read(self: Io) u8 { _ = self; calls += 1; return 2; }
};
const Holder = struct { head: u64, io: Io, tail: u8 };
var calls: u8 = 0;
fn empty() Empty { calls += 1; return .{}; }
fn io() Io { calls += 1; return .{ .impl = empty() }; }
fn holder(value: *Holder) *Holder { calls += 1; return value; }
pub fn main() u8 {
    if (@sizeOf(Empty) != 0 or @sizeOf(Io) != 0 or @alignOf(Io) != 1) return 1;
    if (@offsetOf(Io, "impl") != 0 or @sizeOf(Holder) != 16 or @offsetOf(Holder, "io") != 8) return 2;
    calls = 0;
    var value: Holder = .{ .head = 0x1122334455667788, .io = io(), .tail = 0x9a };
    if (calls != 2) return 3;
    holder(&value).io = io();
    if (calls != 5) return 4;
    holder(&value).io.impl = empty();
    if (calls != 7) return 5;
    const copy = value.io;
    const result = copy.read();
    if (result != 2 or calls != 8) return 6;
    if (value.head != 0x1122334455667788 or value.tail != 0x9a) return 7;
    return 42;
}

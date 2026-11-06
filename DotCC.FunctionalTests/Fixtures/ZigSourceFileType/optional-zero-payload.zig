const Empty = struct {
    fn end(self: *Empty, calls: *u8) void { self.* = .{}; calls.* += 1; }
};
fn value(calls: *u8) Empty { calls.* = calls.* * 10 + 2; return .{}; }
fn maybe(present: bool) ?Empty { return if (present) .{} else null; }
pub fn main() u8 {
    if (@sizeOf(Empty) != 0 or @sizeOf(?Empty) != 1 or @alignOf(?Empty) != 1) return 1;
    if (@sizeOf([3]?Empty) != 3) return 2;
    var optional = maybe(true);
    var calls: u8 = 0;
    if (optional) |*payload| payload.end(&calls) else return 3;
    if (optional == null or calls != 1) return 4;
    calls = 0;
    if (optional) |*payload| payload.* = value(&calls) else return 5;
    if (optional == null or calls != 2) return 6;
    const result = optional orelse return 7;
    _ = result;
    optional = null;
    if (optional != null) return 8;
    var items: [3]?Empty = .{ maybe(false), maybe(true), maybe(false) };
    if (items[0] != null or items[1] == null or items[2] != null) return 9;
    if (items[1]) |*payload| payload.end(&calls) else return 10;
    if (items[0] != null or items[1] == null or items[2] != null or calls != 3) return 11;
    return 42;
}

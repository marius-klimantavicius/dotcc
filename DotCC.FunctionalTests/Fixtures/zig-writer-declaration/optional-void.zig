comptime { @export(&answer, .{ .name = "answer" }); }
fn touch(counter: *u8) void { counter.* += 1; }
fn choose(present: bool, counter: *u8) ?void {
    if (present) return touch(counter);
    return null;
}
pub fn answer() callconv(.c) u8 {
    if (@sizeOf(?void) != 1 or @alignOf(?void) != 1) return 1;
    if (@sizeOf([3]?void) != 3 or @alignOf([3]?void) != 1) return 2;
    const Record = struct { first: u8, value: ?void, last: u8 };
    if (@sizeOf(Record) != 3 or @alignOf(Record) != 1 or @offsetOf(Record, "last") != 2) return 3;
    var counter: u8 = 0;
    var record: Record = .{ .first = 4, .value = choose(true, &counter), .last = 5 };
    if (record.value == null or counter != 1) return 4;
    if (record.value != @as(?void, {})) return 12;
    if (record.value) |*payload| { if (@intFromPtr(payload) != @intFromPtr(&record.value)) return 13; payload.* = {}; }
    if (record.value == null) return 14;
    record.value.?;
    record.value orelse touch(&counter);
    if (counter != 1) return 5;
    record.value = null;
    record.value orelse touch(&counter);
    if (record.value != null or counter != 2) return 6;
    if (choose(true, &counter)) |_| { counter += 1; } else return 7;
    if (choose(false, &counter)) |_| return 8;
    var values: [3]?void = .{ null, {}, null };
    if (values[0] != null or values[1] == null or values[2] != null) return 9;
    if (@intFromPtr(&values[2]) - @intFromPtr(&values[0]) != 2) return 10;
    values[2] = values[1];
    if (values[2] == null or counter != 4) return 11;
    return 42;
}
pub fn main() u8 { return answer(); }

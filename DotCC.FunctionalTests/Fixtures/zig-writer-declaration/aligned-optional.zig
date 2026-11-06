const Payload = struct { number: i96 };
comptime { @export(&answer, .{ .name = "answer" }); }
fn fallback(counter: *u8) Payload { counter.* += 1; return .{ .number = 7 }; }
fn choose(present: bool, counter: *u8) ?Payload {
    counter.* += 1;
    if (present) return .{ .number = 1208925819614629174706176 };
    return null;
}
pub fn answer() callconv(.c) u8 {
    if (@sizeOf(Payload) != 16 or @alignOf(Payload) != 16) return 1;
    if (@sizeOf(?Payload) != 32 or @alignOf(?Payload) != 16) return 2;
    var counter: u8 = 0;
    var optional: ?Payload = choose(true, &counter);
    if (@intFromPtr(&optional) % 16 != 0) return 3;
    const bytes: [*]const u8 = @ptrCast(&optional);
    if (bytes[16] != 1) return 4;
    if ((optional orelse fallback(&counter)).number != 1208925819614629174706176 or counter != 1) return 5;
    if (optional) |*payload| { payload.number += 3; } else return 6;
    if (optional.?.number != 1208925819614629174706179) return 7;
    optional = null;
    if (bytes[16] != 0) return 8;
    if ((optional orelse fallback(&counter)).number != 7 or counter != 2) return 9;
    if ((choose(true, &counter) orelse fallback(&counter)).number != 1208925819614629174706176 or counter != 3) return 10;
    var array: [2]?Payload = .{ null, .{ .number = 42 } };
    if (@intFromPtr(&array[1]) - @intFromPtr(&array[0]) != 32 or @intFromPtr(&array) % 16 != 0) return 11;
    if (array[0] != null or array[1].?.number != 42) return 12;
    const Holder = struct { value: ?Payload };
    var holder: Holder = .{ .value = .{ .number = 42 } };
    if (@sizeOf(Holder) != 32 or @alignOf(Holder) != 16 or @intFromPtr(&holder) % 16 != 0) return 13;
    if (holder.value.?.number != 42) return 14;
    return 42;
}
pub fn main() u8 { return answer(); }

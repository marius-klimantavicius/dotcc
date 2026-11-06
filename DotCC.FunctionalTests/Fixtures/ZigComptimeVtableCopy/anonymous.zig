const Table = struct { first: *const fn () u8, second: *const fn () u8 };
const Service = struct { vtable: *const Table };
fn original() u8 { return 17; }
fn retained() u8 { return 25; }
fn replacement() u8 { return 42; }
const service: Service = .{ .vtable = &.{ .first = original, .second = retained } };
const copied: Table = table: {
    var result = service.vtable.*;
    result.first = replacement;
    break :table result;
};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (service.vtable.first() != 17 or copied.second() != 25) return 1;
    if (copied.first != replacement or copied.second != retained) return 2;
    return copied.first();
}

var small: [1]u3 = .{3};
var byte: [1]u8 = .{8};
var signed: [1]i3 = .{-1};
const Narrow = u3;

fn classify(pointer: anytype) u8 {
    const Child = @typeInfo(@TypeOf(pointer)).pointer.child;
    const name = @typeName(Child);
    if (name.len != 2) return 1;
    if (name[0] == 'u' and name[1] == '3' and @typeInfo(Child).int.bits == 3) return 3;
    if (name[0] == 'u' and name[1] == '8' and @typeInfo(Child).int.bits == 8) return 8;
    if (name[0] == 'i' and name[1] == '3' and @typeInfo(Child).int.bits == 3) return 13;
    return 2;
}

comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const p3: [*]const Narrow = @ptrCast(&small);
    const p8: [*]const u8 = @ptrCast(&byte);
    const pi3: *const i3 = @ptrCast(&signed);
    if (classify(p3) != 3 or classify(p8) != 8 or classify(p3) != 3) return 1;
    if (classify(pi3) != 13) return 2;
    const optional: ?[*]const Narrow = p3;
    const payload = optional orelse return 3;
    if (classify(payload) != 3) return 4;
    return 42;
}

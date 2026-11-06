var storage: u32 = undefined;
const Value = struct { n: u32 };
const one: Value = .{ .n = 1 };
const Descriptor = struct { ptr: *u32, value: *const Value, same: bool = true, immutable: u32 = 1 };
fn descriptor() Descriptor {
    return .{ .ptr = &storage, .value = &one };
}
const known: Descriptor = blk: {
    const value = descriptor();
    const erased: *anyopaque = @ptrCast(value.ptr);
    break :blk .{
        .ptr = @ptrCast(@alignCast(erased)), .value = value.value,
        .same = value.ptr == &storage, .immutable = value.value.n,
    };
};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    storage = 41;
    if (!known.same or known.immutable != 1) return 3;
    if (known.ptr != &storage or known.value != &one) return 1;
    return @intCast(known.ptr.* + known.value.n);
}

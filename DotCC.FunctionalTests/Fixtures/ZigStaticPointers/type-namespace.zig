fn increment(value: u8) u8 { return value + 1; }
fn Wrapper(comptime target: fn (u8) u8) type {
    return struct { pub const call = target; };
}
pub const Namespace: type = blk: { break :blk Wrapper(increment); };

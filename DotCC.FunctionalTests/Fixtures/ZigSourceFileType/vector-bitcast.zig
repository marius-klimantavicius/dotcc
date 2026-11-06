fn spread(flag_bits: u8) u64 {
    const masks: @Vector(8, u8) = .{ 1, 2, 4, 8, 16, 32, 64, 128 };
    const hits = (@as(@Vector(8, u8), @splat(flag_bits)) & masks) == masks;
    const bytes: @Vector(8, u8) = @select(u8, hits,
        @as(@Vector(8, u8), @splat(1)), @as(@Vector(8, u8), @splat(0)));
    return @bitCast(bytes);
}
fn counted(calls: *u32, flag: u8) @Vector(8, u8) {
    calls.* += 1;
    return @splat(flag);
}
pub fn main() u8 {
    var flags: u32 = 0;
    while (flags < 256) : (flags += 1) {
        var storage: [10]u8 = @splat(0xaa);
        @as(*align(1) u64, @ptrCast(&storage[1])).* = bytes: {
            const masks: @Vector(8, u8) = .{ 1, 2, 4, 8, 16, 32, 64, 128 };
            const hits = (@as(@Vector(8, u8), @splat(@as(u8, @intCast(flags)))) & masks) == masks;
            const bytes: @Vector(8, u8) = @select(u8, hits,
                @as(@Vector(8, u8), @splat(1)), @as(@Vector(8, u8), @splat(0)));
            break :bytes @bitCast(bytes);
        };
        if (storage[0] != 0xaa or storage[9] != 0xaa) return 1;
        var lane: u3 = 0;
        while (true) {
            if (storage[@as(usize, lane) + 1] != @as(u8, if (flags & (@as(u32, 1) << lane) != 0) 1 else 0)) return 2;
            if (lane == 7) break;
            lane += 1;
        }
        const back: @Vector(8, u8) = @bitCast(spread(@intCast(flags)));
        if (back[0] != storage[1] or back[7] != storage[8]) return 3;
    }
    var calls: u32 = 0;
    const value: u64 = @bitCast(counted(&calls, 0x12));
    if (value != 0x1212121212121212 or calls != 1) return 4;
    const labeled: u64 = value: { break :value @bitCast(counted(&calls, 0x34)); };
    if (labeled != 0x3434343434343434 or calls != 2) return 5;
    return 42;
}

fn Table(comptime Key: type) type {
    return struct {
        const KeyType = Key;
        const Metadata = packed struct {
            const FingerPrint = u7;
            const free: FingerPrint = 0;
            const tombstone: FingerPrint = 1;
            fingerprint: FingerPrint = free,
            used: u1 = 0,
            const slot_tombstone: u8 = @bitCast(Metadata{ .fingerprint = tombstone });
            const Byte = u8;
            const alias_tombstone: Byte = @bitCast(Metadata{ .fingerprint = tombstone });
            const cached: Byte = blk: { break :blk @as(u8, 1); };
            const recast: Byte = @bitCast(cached);
            fn isTombstone(self: Metadata) bool {
                return @as(u8, @bitCast(self)) == slot_tombstone and slot_tombstone == alias_tombstone
                    and @as(u8, @bitCast(cached)) == recast and recast == 1;
            }
        };
    };
}
const Map = Table(u16);
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const first: Map.Metadata = .{ .fingerprint = 1 };
    const second: Map.Metadata = .{ .fingerprint = 2 };
    return if (first.isTombstone() and !second.isTombstone() and @sizeOf(Map.KeyType) == 2) 42 else 1;
}

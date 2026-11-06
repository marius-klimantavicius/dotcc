var wide: bool = true;
const Value = struct {
    field: switch (if (wide) .wide else .narrow) {
        .wide => u16,
        .narrow => u8,
    },
};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return @intCast(@sizeOf(Value)); }

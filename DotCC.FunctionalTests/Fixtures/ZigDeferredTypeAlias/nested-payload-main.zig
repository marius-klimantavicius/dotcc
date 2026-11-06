const api = @import("nested-payload-api.zig");
const Handler = struct {
    pub const Sequence = union(enum) {
        osc: Osc,
        pub const Osc = api.Command.Unknown;
    };
};
pub fn main() u8 {
    const value: Handler.Sequence = .{ .osc = .{ .value = 42 } };
    return switch (value) { .osc => |payload| @intCast(payload.value) };
}

const api = @import("api.zig");
pub fn main() u8 {
    if (@TypeOf(api.Empty) != type or @TypeOf(api.Switched) != type) return 3;
    if (@sizeOf(api.Empty) != 0 or @alignOf(api.Empty) != 1) return 1;
    const empty: api.Empty = .init;
    const switched: api.Switched = .init;
    const conditional: api.Conditional = .init;
    if (conditional.read() != 42) return 4;
    if (switched.read() != 42) return 2;
    const options = struct { pub const filters: []const u8 = &.{}; };
    inline for (options.filters) |_| @compileError("EMPTY_SLICE_BODY_REACHED");
    return api.forward(u8, empty.read());
}

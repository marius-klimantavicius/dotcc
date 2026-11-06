// Same type-only question shape as pinned std.hash.autoHash's guard.
pub inline fn containsSlice(comptime K: type) bool {
    return switch (@typeInfo(K)) {
        .pointer => |info| info.size == .slice,
        inline .@"struct", .@"union" => |info| {
            inline for (info.field_types) |field_type| {
                if (containsSlice(field_type)) return true;
            }
            return false;
        },
        else => false,
    };
}

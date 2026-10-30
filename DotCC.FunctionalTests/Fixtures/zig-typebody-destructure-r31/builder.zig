// Structural regression for type-body tuples and shared comptime slices.
// The unchanged pinned Ghostty Enum is separately exercised by the strict
// module graph control in libghostty/build/direct-typebody-r31/actual-enum.
pub fn Build(comptime TagInt: type, keys: []const ?[:0]const u8) type {
    var names_raw: [keys.len][]const u8 = undefined;
    var values_raw: [keys.len]comptime_int = undefined;
    const names_actual, const values_actual = kv: {
        var to: comptime_int = 0;
        for (0..keys.len) |from| {
            if (keys[from]) |key| {
                names_raw[to] = key;
                values_raw[to] = from;
                to += 1;
            }
        }
        break :kv .{ names_raw[0..to], values_raw[0..to] };
    };
    return @Enum(TagInt, .exhaustive, names_actual, &(values: {
        var result_int_values: [values_actual.len]TagInt = undefined;
        for (0..values_actual.len) |idx| {
            result_int_values[idx] = values_actual[idx];
        }
        break :values result_int_values;
    }));
}

const left = @import("comptime-array-left.zig");
const right = @import("comptime-array-right.zig");
export fn answer() c_int {
    if (left.value() != 36 or right.value() != 6) return 1;
    if (right.value() != 6 or left.value() != 36) return 2;
    return 42;
}

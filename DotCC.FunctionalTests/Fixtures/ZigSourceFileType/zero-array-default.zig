const provider = @import("flag-defaults.zig");
pub fn main() u8 {
    const len = 2;
    const disabled = provider.Flags.enabled;
    _ = len;
    _ = disabled;
    var value: provider.Stack = .{};
    const other: provider.Stack = .{};
    if (@sizeOf(provider.Stack) != 9 or @alignOf(provider.Stack) != 1) return 1;
    if (@as(u5, @bitCast(value.flags[0])) != 0 or @as(u5, @bitCast(value.flags[7])) != 0) return 2;
    value.flags[7] = .enabled;
    var copy = value;
    copy.flags[7] = .disabled;
    if (@as(u5, @bitCast(value.flags[7])) != 31 or @as(u5, @bitCast(copy.flags[7])) != 0) return 3;
    if (@as(u5, @bitCast(other.flags[7])) != 0) return 4;
    return 42;
}

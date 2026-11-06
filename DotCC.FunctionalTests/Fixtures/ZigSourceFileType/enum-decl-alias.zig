const provider = @import("enum-alias-provider.zig");
fn take(level: provider.Level) u16 { return @intFromEnum(level); }
pub fn main() u8 {
    const Level = u8;
    _ = Level;
    const value: provider.Level = .chained;
    const optional: ?provider.Level = .alias;
    const options: provider.Options = .{};
    if (value != .base or optional.? != .base or options.level != .base) return 1;
    if (take(.alias) != 62 or take(.typed) != 65) return 2;
    if (provider.get() != .base or provider.Level.alias != .base) return 3;
    if (@typeInfo(provider.Level).@"enum".field_names.len != 2) return 4;
    return 42;
}

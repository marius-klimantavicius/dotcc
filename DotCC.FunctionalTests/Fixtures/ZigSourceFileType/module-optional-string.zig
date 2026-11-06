const options = @import("module-optional-string-provider.zig");
pub fn main() u8 {
    if (options.version_pre) |value| { return @intCast(value.len); }
    if (options.version_build) |value| { return if (value.len == 2) 42 else 1; }
    return 2;
}

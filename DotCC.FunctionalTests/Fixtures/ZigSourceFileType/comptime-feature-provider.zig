const Version = struct { major: u32, minor: u32 };
fn supported(comptime requested: Version) bool {
    const available = Version{ .major = 2, .minor = 30 };
    return available.major > requested.major or
        (available.major == requested.major and available.minor >= requested.minor);
}
fn answer(value: u8) u8 { return value + 2; }
pub const present = if (supported(.{ .major = 2, .minor = 25 })) answer else {};
pub const absent = if (supported(.{ .major = 3, .minor = 0 })) answer else {};
pub const scalar: u8 = if (supported(.{ .major = 2, .minor = 30 })) 42 else @compileError("dead typed arm");
pub fn select(flag: bool) u8 { return if (flag) 40 else 2; }

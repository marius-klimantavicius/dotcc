fn width(comptime enabled: bool) ?comptime_int {
    return if (enabled) 16 else null;
}

fn count(comptime enabled: bool) usize {
    if (comptime width(enabled)) |n| {
        const Block = [n]u8;
        return @sizeOf(Block) + 26;
    }
    return 42;
}

pub fn main() u8 {
    if (count(true) != 42) return 1;
    if (count(false) != 42) return 2;
    const explicit = if (comptime (width(true))) |n| n + 26 else 1;
    if (explicit != 42) return 3;
    const absent = if (comptime width(false)) |n| n else 42;
    if (absent != 42) return 4;
    const implicit = if (width(true)) |n| n + 26 else 1;
    if (implicit != 42) return 5;
    return 42;
}

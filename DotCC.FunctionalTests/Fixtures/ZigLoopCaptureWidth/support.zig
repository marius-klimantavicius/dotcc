fn take(value: u21) u8 { return if (value == 65) 42 else 1; }
pub fn through(args: anytype) u8 {
    if (@bitSizeOf(@TypeOf(args[0])) != 21) return 2;
    return take(args[0]);
}

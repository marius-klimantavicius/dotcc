pub fn value() u8 {
    const data = comptime table: {
        break :table [_]u8{ 17, 19 };
    };
    return data[0] + data[1];
}

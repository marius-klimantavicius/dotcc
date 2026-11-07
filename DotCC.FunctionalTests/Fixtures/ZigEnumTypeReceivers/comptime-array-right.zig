pub fn value() u32 {
    const data = comptime table: {
        break :table [_]u32{ 1, 2, 3 };
    };
    return data[0] + data[1] + data[2];
}

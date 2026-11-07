const Code = enum(u8) {
    invalid = 0,
    small = 3,
    wide = 17,
    _,
    pub fn Type(comptime self: Code) type {
        return switch (self) { .small => u16, .wide => u32, .invalid, _ => void };
    }
};
var calls: usize = 0;
fn next(code: u8) Code {
    calls += 1;
    return @enumFromInt(code);
}
fn typed(comptime code: Code) c_int {
    const T = code.Type();
    return if (@typeInfo(T).int.bits == 16) 31 else 42;
}
fn route(code: u8) c_int {
    return switch (next(code)) {
        .invalid => 10,
        _ => 20,
        inline else => |tag| typed(tag),
    };
}
fn reversed(code: u8) c_int {
    return switch (next(code)) {
        inline else => |tag| typed(tag),
        _ => 20,
        .invalid => 10,
    };
}
export fn answer() c_int {
    for (0..256) |i| {
        const expected: c_int = switch (i) { 0 => 10, 3 => 31, 17 => 42, else => 20 };
        if (route(@intCast(i)) != expected or reversed(@intCast(i)) != expected) return 1;
    }
    if (calls != 512) return 2;
    return 42;
}

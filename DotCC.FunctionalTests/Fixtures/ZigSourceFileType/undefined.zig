const Payload = struct { pointer: ?*u8, values: []u8, value: u8 };
fn Box(comptime T: type, comptime initial: T) type {
    return struct {
        pub fn answer() u8 {
            var local: @TypeOf(initial) = initial;
            local.pointer = null;
            local.values = &.{};
            local.value = 42;
            return local.value;
        }
    };
}
fn Fields() type {
    var values: [2]u16 = undefined;
    values[0] = 20;
    values[1] = 22;
    const total = values[0] + values[1];
    return struct { pub const sum = total; };
}
pub fn main() u8 {
    const B = Box(Payload, undefined);
    if (Fields().sum != 42) return 1;
    return B.answer();
}

fn Container(comptime T: type) type {
    return struct {
        pub fn set(element: T) T {
            const chosen = switch (@typeInfo(T)) {
                .@"struct" => element,
                .@"union" => T.nonexistentConversion(element),
                else => @compileError("unsupported test type"),
            };
            return chosen;
        }
    };
}
fn identity(value: anytype) @TypeOf(value) { return value; }
fn callSelected(comptime T: type, value: T) T {
    const operation = switch (@typeInfo(T)) {
        .int => identity,
        else => @compileError("inactive function alias"),
    };
    return operation(value);
}
pub fn main() u8 {
    const Row = struct { value: u8 };
    const row = Container(Row).set(.{ .value = 40 });
    return row.value + callSelected(u8, 2);
}

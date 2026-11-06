const Missing = if (true) noreturn else struct { reset_attributes: u8 };
const Value = union(enum) { live: u8, empty, absent: Missing };
var calls: u8 = 0;
fn next() Value {
    calls += 1;
    return .{ .live = 40 };
}
fn expression(value: Value) u8 {
    return switch (value) {
        .live => |number| number,
        .empty => 0,
        .absent => |missing| missing.reset_attributes,
    };
}
fn statement(value: Value) u8 {
    switch (value) {
        .live => |number| return number,
        .empty => return 0,
        .absent => @compileError("an uninhabited payload has no runtime arm"),
    }
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const number = expression(next());
    return number + calls + statement(.{ .live = 1 });
}

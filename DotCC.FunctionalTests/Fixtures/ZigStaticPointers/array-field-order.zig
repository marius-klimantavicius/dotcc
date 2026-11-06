var trace: u32 = 0;
fn step(value: u8) u8 { trace = trace * 10 + value; return value; }
const Element = struct { value: u8 };
const Record = struct { first: u8, elements: [3]Element, last: u8 };
fn make() Record {
    return .{ .first = step(1), .elements = @splat(.{ .value = step(2) }), .last = step(3) };
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var value = make();
    if (trace != 123 or value.first != 1 or value.last != 3) return 1;
    for (value.elements) |element| { if (element.value != 2) return 2; }
    value.elements[0].value = 4;
    if (value.elements[1].value != 2 or value.elements[2].value != 2) return 3;
    return 42;
}

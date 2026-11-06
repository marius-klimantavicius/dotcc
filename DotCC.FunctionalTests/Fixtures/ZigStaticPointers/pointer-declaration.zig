const Value = struct {
    value: u8,
    fn of(pointer: *Value) *Value { return pointer; }
};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var value: Value = .{ .value = 42 };
    const pointer: *Value = .of(&value);
    return pointer.value;
}

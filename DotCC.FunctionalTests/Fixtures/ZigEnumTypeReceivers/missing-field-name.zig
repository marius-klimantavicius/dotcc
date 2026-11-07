const Record = struct { x: u8 };
fn put(comptime T: type, comptime name: []const u8, p: *T, value: @FieldType(T, name)) void {
    @field(p, name) = value;
}
export fn answer() c_int {
    var value: Record = .{ .x = 0 };
    put(Record, "missing", &value, 0);
    return 42;
}

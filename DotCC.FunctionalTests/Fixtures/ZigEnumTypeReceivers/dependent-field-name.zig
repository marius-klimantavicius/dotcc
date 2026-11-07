const Record = struct { small: u3, large: u21 };
fn put(comptime T: type, comptime name: []const u8, p: *T, value: @FieldType(T, name)) void {
    @field(p, name) = value;
}
export fn answer() c_int {
    const name = "wrong";
    var value: Record = .{ .small = 0, .large = 0 };
    put(Record, "small", &value, 7);
    put(Record, "large", &value, 123456);
    if (value.small != 7 or value.large != 123456 or name[0] != 'w') return 1;
    return 42;
}

fn CEnum(comptime E: type) type {
    const info = @typeInfo(E).@"enum";
    const values = comptime values: {
        var result: [info.field_values.len]info.tag_type = undefined;
        for (&result, info.field_values) |*dst, value| dst.* = value;
        break :values result;
    };
    return @Enum(info.tag_type, .nonexhaustive, info.field_names, &values);
}
const Tag = enum(i16) { negative = -7, zero = 0, sparse = 300 };
const Open = CEnum(Tag);
const OtherTag = enum(u8) { flag = 5 };
const OtherOpen = CEnum(OtherTag);
fn route(n: i16) c_int {
    const value: Open = @enumFromInt(n);
    return switch (value) { .negative => 11, .zero => 12, .sparse => 13, _ => 14 };
}
export fn answer() c_int {
    if (@backingInt(OtherOpen.flag) != 5 or @sizeOf(OtherOpen) != 1) return 4;
    if (@backingInt(Open.negative) != -7 or @backingInt(Open.sparse) != 300) return 1;
    if (route(-7) != 11 or route(0) != 12 or route(300) != 13) return 2;
    if (route(-8) != 14 or route(299) != 14) return 3;
    return 42;
}

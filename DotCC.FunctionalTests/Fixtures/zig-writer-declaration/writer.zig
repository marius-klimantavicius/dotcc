const std = @import("std");
fn Copy(comptime T: type) type {
    const info = @typeInfo(T).@"struct";
    var types: [info.field_names.len]type = undefined;
    inline for (info.field_types, &types) |Type, *out| { out.* = Type; }
    return @Struct(.auto, null, info.field_names, &types, &@splat(.{}));
}
const Values = Copy(struct { a: u1, b: u7, c: u8, d: u17, e: u32, f: i7 });
comptime { @export(&formatAnswer, .{ .name = "formatAnswer" }); }
pub fn formatAnswer() callconv(.c) u8 {
    const values: Values = .{ .a = 1, .b = 127, .c = 255, .d = 65537, .e = 4294967295, .f = -12 };
    var buffer: [128]u8 = undefined;
    var writer: std.Io.Writer = .fixed(&buffer);
    writer.print("{d},{d},{d},{d},{d},{d}", .{ values.a, values.b, values.c, values.d, values.e, values.f }) catch return 1;
    const actual = writer.buffered();
    const expected = "1,127,255,65537,4294967295,-12";
    if (actual.len != expected.len) return 2;
    for (actual, expected) |a, b| { if (a != b) return 3; }
    return 42;
}

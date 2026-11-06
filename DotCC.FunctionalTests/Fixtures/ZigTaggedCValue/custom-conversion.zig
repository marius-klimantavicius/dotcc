const lib = @import("unions");
const payloads = @import("payloads.zig");
const Tag = enum(c_int) { empty, number, rgb, unknown, mode, plain };
const Value = union(Tag) {
    empty,
    number: u32,
    rgb: payloads.RGB,
    unknown: payloads.Unknown,
    mode: payloads.Mode,
    plain: payloads.Plain,
};
fn Api() type { return lib.TaggedUnion(.c, Value, .{ .padding = [8]u64 }); }
fn convert(value: Value) Api().C { return Api().cval(value); }
var productions: u32 = 0;
fn produce() Value {
    productions += 1;
    return .{ .rgb = .{ .r = 4, .g = 5, .b = 6 } };
}

comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const color = convert(produce());
    if (color.tag != .rgb or color.value.rgb.red != 5 or color.value.rgb.green != 7 or
        color.value.rgb.blue != 9 or color.value.rgb.error_code != 0) return 1;
    if (productions != 1 or payloads.conversions != 1 or payloads.rejected != 0) return 2;
    const bad = convert(.{ .rgb = .{ .r = 255, .g = 10, .b = 11 } });
    if (bad.value.rgb.red != 255 or bad.value.rgb.blue != 11 or
        bad.value.rgb.error_code != 71 or payloads.rejected != 1) return 3;
    const full = [_]u16{ 11, 22, 33, 44 };
    const partial = full[2..];
    const unknown = convert(.{ .unknown = .{ .full = &full, .partial = partial } });
    if (unknown.tag != .unknown or unknown.value.unknown.full_len != 4 or
        unknown.value.unknown.partial_len != 2 or unknown.value.unknown.full_ptr != &full or
        unknown.value.unknown.partial_ptr != partial.ptr or unknown.value.unknown.partial_ptr[0] != 33) return 4;
    const mode = convert(.{ .mode = .on });
    if (mode.tag != .mode or mode.value.mode != 39 or payloads.conversions != 4) return 5;
    const empty = convert(.empty);
    const number = convert(.{ .number = 42 });
    const plain = convert(.{ .plain = .{ .value = 41 } });
    if (empty.tag != .empty or number.value.number != 42 or plain.value.plain.value != 41 or
        payloads.conversions != 4) return 6;
    return @intCast(number.value.number);
}

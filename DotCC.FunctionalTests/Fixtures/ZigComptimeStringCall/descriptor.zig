const std = @import("std");
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const Encoder = std.base64.standard.Encoder;
    const address = &Encoder;
    var buffer: [Encoder.calcSize(5)]u8 = undefined;
    const encoded = Encoder.encode(&buffer, "hello");
    if (encoded.len != 8 or !std.mem.eql(u8, encoded, "aGVsbG8=")) return 1;
    if (address != &Encoder) return 2;
    var copy = Encoder;
    copy.pad_char = null;
    if (copy.calcSize(5) != 7 or Encoder.calcSize(5) != 8) return 3;
    return 42;
}

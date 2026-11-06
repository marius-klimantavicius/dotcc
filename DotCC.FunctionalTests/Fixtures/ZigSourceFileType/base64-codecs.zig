const base64 = @import("std").base64;
const copied = base64.standard;
const callback = copied.decoderWithIgnore;
const ignored = callback(" \n");
const raw = "\xfb\xff";
const chunk_size = 3072;
fn same(a: []const u8, b: []const u8) bool {
    if (a.len != b.len) return false;
    for (a, b) |x, y| { if (x != y) return false; }
    return true;
}
fn check(codecs: base64.Codecs, expected: []const u8, spaced: []const u8) bool {
    var encoded: [16]u8 = undefined;
    if (!same(codecs.Encoder.encode(&encoded, raw), expected)) return false;
    var decoded: [2]u8 = undefined;
    if ((codecs.Decoder.calcSizeForSlice(expected) catch return false) != raw.len) return false;
    codecs.Decoder.decode(&decoded, expected) catch return false;
    if (!same(&decoded, raw)) return false;
    const decoder = codecs.decoderWithIgnore(" \n");
    const count = decoder.decode(&decoded, spaced) catch return false;
    return count == raw.len and same(&decoded, raw);
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var chunk: [base64.standard.Encoder.calcSize(chunk_size)]u8 = undefined;
    chunk[0] = 42;
    if (chunk.len != 4096) return 1;
    if (base64.standard_no_pad.Encoder.calcSize(2) != 3 or base64.url_safe.Encoder.calcSize(2) != 4) return 2;
    if (copied.decoderWithIgnore != base64.standard.decoderWithIgnore or callback != base64.standard_no_pad.decoderWithIgnore) return 3;
    if (callback == base64.url_safe.decoderWithIgnore) return 4;
    if (ignored.decoder.pad_char != '=' or !ignored.char_is_ignored[' '] or !ignored.char_is_ignored['\n']) return 5;
    if (!check(base64.standard, "+/8=", "+ /8=\n")) return 6;
    // The original Codecs intentionally share each alphabet's callback:
    // standard callbacks expect padding; URL-safe callbacks omit padding.
    if (!check(base64.standard_no_pad, "+/8", "+ /8=\n")) return 7;
    if (!check(base64.url_safe, "-_8=", "- _8\n")) return 8;
    if (!check(base64.url_safe_no_pad, "-_8", "- _8\n")) return 9;
    var encoded: [4]u8 = undefined;
    if (!same(base64.standard.Encoder.encode(&encoded, "f"), "Zg==")) return 10;
    if (!same(base64.standard_no_pad.Encoder.encode(&encoded, "f"), "Zg")) return 11;
    if (!same(base64.url_safe.Encoder.encode(&encoded, "foo"), "Zm9v")) return 12;
    if (copied.Decoder.fast_char_to_index[0]['B'] != 4 or copied.Decoder.fast_char_to_index[1]['B'] != 4096 or
        copied.Decoder.fast_char_to_index[2]['B'] != 4194304 or copied.Decoder.fast_char_to_index[3]['B'] != 65536) return 13;
    const long_raw = "foobarfoobarfoobar";
    const long_encoded = "Zm9vYmFyZm9vYmFyZm9vYmFy";
    var long_buffer: [32]u8 = undefined;
    if (!same(base64.standard.Encoder.encode(&long_buffer, long_raw), long_encoded)) return 14;
    var long_decoded: [18]u8 = undefined;
    base64.standard.Decoder.decode(&long_decoded, long_encoded) catch return 15;
    if (!same(&long_decoded, long_raw)) return 16;
    return chunk[0];
}

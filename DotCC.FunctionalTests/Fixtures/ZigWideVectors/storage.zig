const Wide = @Vector(128, u8);
var value_calls: usize = 0;
var pointer_calls: usize = 0;

fn nextByte() u8 {
    value_calls += 1;
    return 0x5c;
}

fn pointer(p: [*]u8) [*]u8 {
    pointer_calls += 1;
    return p;
}

// The full-width loop, overlapping tail, and smaller pairs of stores used by
// a vectorized byte fill. No target/mcpu override is needed for this fixture.
fn fill(d: [*]u8, byte: u8, len: usize) void {
    if (len >= 128) {
        const splat: Wide = @splat(byte);
        var i: usize = 0;
        while (i + 128 <= len) : (i += 128) {
            d[i..][0..128].* = splat;
            asm volatile ("" ::: .{ .memory = true });
        }
        if (i != len) d[len - 128 ..][0..128].* = splat;
        return;
    }
    if (len >= 64) {
        const splat: @Vector(64, u8) = @splat(byte);
        d[0..64].* = splat;
        d[len - 64 ..][0..64].* = splat;
        return;
    }
    if (len >= 16) {
        const splat: @Vector(16, u8) = @splat(byte);
        d[0..16].* = splat;
        d[len - 16 ..][0..16].* = splat;
        if (len > 32) {
            d[16..32].* = splat;
            d[len - 32 ..][0..16].* = splat;
        }
        return;
    }
    if (len >= 8) {
        const splat: @Vector(8, u8) = @splat(byte);
        d[0..8].* = splat;
        d[len - 8 ..][0..8].* = splat;
        return;
    }
    if (len >= 4) {
        const splat: @Vector(4, u8) = @splat(byte);
        d[0..4].* = splat;
        d[len - 4 ..][0..4].* = splat;
        return;
    }
    var i: usize = 0;
    while (i < len) : (i += 1) {
        d[i] = byte;
        asm volatile ("" ::: .{ .memory = true });
    }
}

pub fn main() u8 {
    if (@sizeOf(Wide) != 128 or @sizeOf(@Vector(4, u8)) != 4) return 1;
    var buf: [546]u8 = undefined;
    var offset: usize = 0;
    while (offset < 129) : (offset += 1) {
        var len: usize = 0;
        while (len <= 416) : (len += 1) {
            var c: usize = 0;
            while (c < 3) : (c += 1) {
                const byte: u8 = if (c == 0) 0 else if (c == 1) 0x5c else 0xff;
                var k: usize = 0;
                while (k < buf.len) : (k += 1) buf[k] = 0xaa;
                fill(buf[offset..].ptr, byte, len);
                k = 0;
                while (k < buf.len) : (k += 1) {
                    const expected: u8 = if (k >= offset and k < offset + len) byte else 0xaa;
                    if (buf[k] != expected) return 2;
                }
            }
        }
    }
    // Nonuniform upper/lower halves expose truncation and incorrect lane order.
    var k: usize = 0;
    while (k < buf.len) : (k += 1) buf[k] = @truncate(k * 37 + 11);
    const loaded: Wide = pointer(buf[1..].ptr)[0..128].*;
    var copied = loaded;
    copied[64] = 0x12;
    copied[127] = 0x34;
    pointer(buf[259..].ptr)[0..128].* = copied;
    if (pointer_calls != 2) return 3;
    inline for (0..128) |lane| {
        const expected: u8 = @truncate((lane + 1) * 37 + 11);
        if (loaded[lane] != expected) return 4;
        const changed: u8 = if (lane == 64) 0x12 else if (lane == 127) 0x34 else expected;
        if (buf[259 + lane] != changed) return 5;
    }
    if (buf[258] != @as(u8, @truncate(258 * 37 + 11))) return 6;
    if (buf[387] != @as(u8, @truncate(387 * 37 + 11))) return 7;
    const splat: Wide = @splat(nextByte());
    pointer(buf[3..].ptr)[0..128].* = splat;
    if (value_calls != 1 or pointer_calls != 3) return 8;
    k = 0;
    while (k < 128) : (k += 1) if (buf[3 + k] != 0x5c) return 9;
    const small: @Vector(4, u8) = buf[1..5].*;
    buf[400..404].* = small;
    inline for (0..4) |lane| if (small[lane] != buf[1 + lane] or buf[400 + lane] != small[lane]) return 10;
    // The storage width stays 128 bytes with larger numeric lanes too.
    var words: [34]u32 = undefined;
    const words_vec: @Vector(32, u32) = @splat(0x12345678);
    words[0] = 7;
    words[33] = 9;
    words[1..33].* = words_vec;
    const words_loaded: @Vector(32, u32) = words[1..33].*;
    inline for (0..32) |lane| if (words_loaded[lane] != 0x12345678) return 11;
    if (words[0] != 7 or words[33] != 9) return 12;
    const normal: @Vector(16, u8) = @splat(3);
    if (!@reduce(.And, normal == @as(@Vector(16, u8), @splat(3)))) return 13;
    const added = normal + normal;
    if (added[15] != 6) return 14;
    return 0;
}

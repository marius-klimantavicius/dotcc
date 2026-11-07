const std = @import("std");
const before = @import("original").c_style;
const after = @import("adapted").c_style;

fn defaultAgrees(bits: u128) bool {
    var a: before.Style = undefined;
    var b: after.Style = undefined;
    @as(*align(@alignOf(before.Style)) u128, @ptrCast(&a)).* = bits;
    @as(*align(@alignOf(after.Style)) u128, @ptrCast(&b)).* = bits;
    return before.style_is_default(&a) == after.style_is_default(&b);
}

fn core(comptime M: type, seed: u128) M.CoreStyle {
    return .{
        .fg_color = .rgb(.{ .r = @truncate(seed), .g = @truncate(seed >> 8), .b = @truncate(seed >> 16) }),
        .bg_color = .palette(@truncate(seed >> 24)),
        .underline_color = .rgb(.{ .r = @truncate(seed >> 32), .g = @truncate(seed >> 40), .b = @truncate(seed >> 48) }),
        .flags = @bitCast(@as(u16, @truncate(seed & 255)) | (@as(u16, @intCast(seed % 6)) << 8) |
            (@as(u16, @intCast((seed >> 8) & 31)) << 11)),
        ._padding = @truncate(seed >> 64),
    };
}

pub fn main() u8 {
    const mask = before.probe_mask;
    if (mask != after.probe_mask) return 1;
    if (before.style_is_default(null) != after.style_is_default(null)) return 2;
    var a: before.Style = undefined;
    var b: after.Style = undefined;
    before.default_style(&a);
    after.default_style(&b);
    if (!std.mem.eql(u8, &std.mem.toBytes(a), &std.mem.toBytes(b))) return 3;
    if (!defaultAgrees(0) or !defaultAgrees(mask)) return 4;
    for (0..128) |bit| {
        const basis = @as(u128, 1) << @intCast(bit);
        if (!defaultAgrees(basis) or !defaultAgrees(basis | mask)) return 5;
        @as(*align(@alignOf(after.Style)) u128, @ptrCast(&b)).* = basis;
        if (after.style_is_default(&b) != ((basis & ~mask) == 0)) return 6;
    }
    var random: u128 = 0x123456789abcdef0fedcba9876543210;
    for (0..4096) |_| {
        random ^= random << 13;
        random ^= random >> 7;
        random ^= random << 17;
        if (!defaultAgrees(random) or !defaultAgrees(random & mask) or !defaultAgrees(random & ~mask)) return 7;
        @as(*align(@alignOf(after.Style)) u128, @ptrCast(&b)).* = random & mask;
        if (!after.style_is_default(&b)) return 8;
        before.Style.write(core(before, random), &a);
        after.Style.write(core(after, random), &b);
        if (!std.mem.eql(u8, &std.mem.toBytes(a), &std.mem.toBytes(b))) return 9;
        const aa = before.Style.fromStyle(core(before, random));
        const bb = after.Style.fromStyle(core(after, random));
        if (!std.mem.eql(u8, &std.mem.toBytes(aa), &std.mem.toBytes(bb))) return 10;
    }
    std.debug.print("mask={x} basis=128 random=4096 writes=4096 conversions=4096\n", .{mask});
    return 42;
}

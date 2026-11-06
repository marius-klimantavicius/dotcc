const Backing = u32;

fn Offset(comptime T: type) type {
    return packed struct(Backing) {
        value: Backing = 0,
        pub fn element() type { return T; }
    };
}

fn Identity(comptime T: type) type { return T; }
fn Narrow(comptime BackingType: type) type {
    return Identity(packed struct(BackingType) { value: BackingType = 0 });
}
fn Outer(comptime T: type) type {
    return struct {
        pub fn Make() type { return packed struct(T) { value: T = 0 }; }
    };
}

pub fn main() u8 {
    const P = Offset(u16);
    const Again = Offset(u16);
    const Other = Offset(u8);
    if (P != Again or P == Other) return 1;
    if (@sizeOf(P) != 4 or @bitSizeOf(P) != 32 or @alignOf(P) != 4) return 2;
    var p: P = .{};
    if (p.value != 0) return 3;
    p.value = 0x12345678;
    const bytes: [*]const u8 = @ptrCast(&p);
    if (bytes[0] != 0x78 or bytes[1] != 0x56 or bytes[2] != 0x34 or bytes[3] != 0x12) return 4;
    const N = Narrow(u3);
    const W = Narrow(u5);
    if (N == W or @bitSizeOf(N) != 3 or @bitSizeOf(W) != 5) return 5;
    var n: N = .{};
    n.value = 5;
    const M = Outer(u9).Make();
    if (@bitSizeOf(M) != 9 or @sizeOf(M) != 2) return 6;
    const Local = packed struct(u8) { low: u3 = 0, high: u5 = 0 };
    var local: Local = .{};
    local.low = n.value;
    local.high = 17;
    const raw: *const u8 = @ptrCast(&local);
    if (raw.* != 141) return 7;
    return 42;
}

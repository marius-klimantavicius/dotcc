fn probe1(x: u1) u8 {
    const signed: i1 = @bitCast(x);
    const roundtrip: u1 = @bitCast(signed);
    return if (signed == -1 and roundtrip == x) 0 else 1;
}

fn probe3(x: u3) u8 {
    const signed: i3 = @bitCast(x);
    const roundtrip: u3 = @bitCast(signed);
    return if (signed == -1 and roundtrip == x) 0 else 3;
}

fn probe7(x: u7) u8 {
    const signed: i7 = @bitCast(x);
    const roundtrip: u7 = @bitCast(signed);
    return if (signed == -1 and roundtrip == x) 0 else 7;
}

fn probe9(x: u9) u8 {
    const signed: i9 = @bitCast(x);
    const roundtrip: u9 = @bitCast(signed);
    return if (signed == -1 and roundtrip == x) 0 else 9;
}

fn probe24(x: u24) u8 {
    const signed: i24 = @bitCast(x);
    const roundtrip: u24 = @bitCast(signed);
    return if (signed == -1 and roundtrip == x) 0 else 24;
}

fn probe33(x: u33) u8 {
    const signed: i33 = @bitCast(x);
    const roundtrip: u33 = @bitCast(signed);
    return if (signed == -1 and roundtrip == x) 0 else 33;
}

fn probe65(x: u65) u8 {
    const signed: i65 = @bitCast(x);
    const roundtrip: u65 = @bitCast(signed);
    return if (signed == -1 and roundtrip == x) 0 else 65;
}

pub fn main() u8 {
    if (probe1(1) != 0) return 1;
    if (probe3(7) != 0) return 3;
    if (probe7(127) != 0) return 7;
    if (probe9(511) != 0) return 9;
    if (probe24(16777215) != 0) return 24;
    if (probe33(8589934591) != 0) return 33;
    if (probe65(36893488147419103231) != 0) return 65;
    return 0;
}

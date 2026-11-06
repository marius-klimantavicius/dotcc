fn answer(comptime name: @EnumLiteral()) u8 {
    return if (name == .one) 20 else 22;
}
fn Scoped(comptime name: @EnumLiteral()) type {
    return struct {
        pub const label = @tagName(name);
        pub fn amount() u8 { return answer(name); }
    };
}
pub fn main() u8 {
    const A = Scoped(.one);
    const B = Scoped(.second);
    if (A.label.len != 3 or B.label.len != 6) return 1;
    return A.amount() + B.amount();
}

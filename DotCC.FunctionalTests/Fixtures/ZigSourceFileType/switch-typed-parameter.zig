const Kind = enum { small, large };
fn tag(kind: Kind) Kind { return kind; }
fn number(comptime kind: Kind, value: switch (tag(kind)) { .small => u7, .large => u16 }) u16 {
    return value;
}
pub fn main() u8 {
    return @intCast(number(.small, 7) + number(.large, 35));
}

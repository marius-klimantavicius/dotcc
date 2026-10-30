fn retain() @TypeOf(.{ "abcd", @as(u8, 7) }) { return .{ "abcd", @as(u8, 7) }; }
fn consume(value: anytype) u8 { const p = value.@"0"; return p[0] + p[3] - value.@"1"; }
pub fn main() u8 { const value = retain(); return consume(value); }

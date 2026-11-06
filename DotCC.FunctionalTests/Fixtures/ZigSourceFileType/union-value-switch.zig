const Color = union(Tag) {
    none: void,
    palette: u8,
    rgb: u32,
    const Tag = enum(u8) { none, palette, rgb };
    fn eql(self: Color, other: Color) bool {
        return @as(Tag, self) == @as(Tag, other) and switch (self) {
            .none => true,
            .palette => self.palette == other.palette,
            .rgb => self.rgb == other.rgb,
        };
    }
};
fn make(calls: *u8) Color { calls.* += 1; return .{ .palette = 17 }; }
fn selected(calls: *u8) u8 {
    return 1 + switch (make(calls)) { .none => @as(u8, 1), .palette => @as(u8, 41), .rgb => @as(u8, 2) };
}
fn payload(value: Color) u32 {
    return 1 + switch (value) { .none => @as(u32, 0), .palette => |p| @as(u32, p), .rgb => |rgb| rgb };
}
fn lazy(calls: *u8, enabled: bool) bool {
    return enabled and switch (make(calls)) { .none => false, .palette => |p| p == 17, .rgb => |rgb| rgb == 17 };
}
pub fn main() u8 {
    const first: Color = .{ .palette = 17 };
    const second: Color = .{ .palette = 18 };
    const third: Color = .{ .rgb = 17 };
    const fourth: Color = .{ .rgb = 18 };
    const none: Color = .none;
    if (!first.eql(first) or first.eql(second) or first.eql(third)) return 1;
    if (!third.eql(third) or third.eql(fourth) or third.eql(none)) return 2;
    if (!none.eql(none) or none.eql(first)) return 3;
    var calls: u8 = 0;
    const answer = selected(&calls);
    if (calls != 1) return 4;
    if (payload(first) != 18 or payload(third) != 18 or payload(none) != 1) return 5;
    if (lazy(&calls, false) or calls != 1) return 6;
    if (!lazy(&calls, true) or calls != 2) return 7;
    return answer;
}

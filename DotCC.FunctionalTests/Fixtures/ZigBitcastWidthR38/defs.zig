pub const Word = u24;
pub const Trio = [3]u3;
pub const Holder = struct {
    value: Word = @bitCast(@as(i24, -1)),
    lanes: Trio,
    pub fn take(self: *const @This(), x: Word) u8 {
        return if (self.value == x) 0 else 1;
    }
};
pub fn take(x: Word) u8 { return if (x == 0xffffff) 0 else 1; }
pub fn lanes(x: Trio) u9 { return @bitCast(x); }
pub fn generic(comptime T: type, x: T) T { return @bitCast(x); }
pub fn recode(comptime T: type, x: anytype) T { return @bitCast(x); }

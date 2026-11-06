fn Build() type {
    const text: []const u8 = "value";
    const bytes = [_]u8{ 'v', 'a', 'l', 'u', 'e' };
    const pointer = &bytes;
    return struct {
        value: u8,
        pub const answer: u8 = 42;
        pub fn answerValue() u8 {
            if (!@hasField(@This(), text)) return 1;
            if (!@hasField(@This(), pointer)) return 2;
            const instance = @This(){ .value = 42 };
            return @field(instance, text);
        }
    };
}
const Result = Build();
pub fn main() u8 { return Result.answerValue(); }

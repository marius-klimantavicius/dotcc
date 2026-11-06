pub const Result = enum(u8) { answer = 42 };
fn implementation(value: u8) Result { return @enumFromInt(value); }
pub const Options = struct {
    pub const T = u8;
    pub const typed: fn (T) Result = implementation;
};
pub const forwarded = Options.typed;

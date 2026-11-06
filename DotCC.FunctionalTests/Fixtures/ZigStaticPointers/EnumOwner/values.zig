pub const Small = enum(u16) { low = 6, high = 1025 };
pub const calling_conv: @import("std").lang.CallingConvention = .c;
pub const Signed = enum(i7) { negative = -12, positive = 34 };
fn make() type { return @Enum(u16, .exhaustive, &.{ "low", "high" }, &.{ 6, 1025 }); }
pub const Reified = make();

pub const Set = Make(Value);
pub const Early = enum(u8) { selected = @sizeOf(Set) };
pub const Value = struct { value: u8 };
fn Make(comptime T: type) type { return struct { value: T }; }

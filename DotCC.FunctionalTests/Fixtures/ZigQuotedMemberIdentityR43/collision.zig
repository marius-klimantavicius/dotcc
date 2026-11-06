const Target = struct { @"a-b": u8 };
pub fn main() u8 { return if (@hasField(Target, "a?b")) 1 else 42; }

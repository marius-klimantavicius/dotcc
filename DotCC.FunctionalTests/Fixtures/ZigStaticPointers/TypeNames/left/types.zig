pub const Request = union(enum) { none, number: u8 };
pub const Outer = struct { pub const Inner = struct { value: u8 }; };
pub const Alias = Request;

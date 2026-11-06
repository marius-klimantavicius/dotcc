pub const AnyFuture = opaque {};
pub const Alias = AnyFuture;
pub const OtherFuture = opaque {};
pub const Nested = struct { pub const Handle = opaque {}; };
pub const NestedAlias = Nested.Handle;

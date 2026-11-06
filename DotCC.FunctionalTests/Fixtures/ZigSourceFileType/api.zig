pub const Empty = @import("empty.zig");
const impl = @import("impl.zig");
pub const forward = impl.forward;
const Choice = enum { first, second };
const choice: Choice = .first;
const Broken = @Int(.unsigned, @compileError("UNSELECTED_TYPE_BRANCH_ANALYZED"));
pub const Conditional = switch (choice) { .first => if (false) Broken else Empty, .second => Broken };
pub const Switched = switch (choice) {
    .first => @import("empty.zig"),
    .second => @import("must-stay-unloaded.zig"),
};

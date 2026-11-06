const root = @import("root");
comptime {
    if (!@hasDecl(root, "later")) @compileError("root header must be present");
    if (@typeInfo(@TypeOf(root.later)) != .@"fn") @compileError("real root function type required");
}
pub const marker: u8 = 1;

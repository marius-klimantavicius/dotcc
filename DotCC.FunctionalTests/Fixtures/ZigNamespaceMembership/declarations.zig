const Self = @This();
pub const Public = 1;
const Private = 2;
pub fn function(comptime T: type) type { if (T == u128) @compileError("membership must not demand this body"); return T; }
fn privateFunction() void {}
pub fn own() bool { return @hasDecl(Self, "Private") and @hasDecl(Self, "privateFunction"); }

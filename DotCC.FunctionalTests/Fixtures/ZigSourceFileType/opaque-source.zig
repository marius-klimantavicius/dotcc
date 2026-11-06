const owner = @import("opaque-owner.zig");
const reexport = @import("opaque-reexport.zig");
const Future = owner.AnyFuture;
const AnyFuture = opaque {};
const VTable = struct {
    identity: *const fn (*Future) *Future,
    maybe: *const fn (?*Future) ?*Future,
    acquire: *const fn (*Future) error{Failed}!*Future,
};
fn identity(pointer: *Future) *Future { return pointer; }
fn maybe(pointer: ?*Future) ?*Future { return pointer; }
fn acquire(pointer: *Future) error{Failed}!*Future { return pointer; }
fn genericIdentity(comptime T: type, pointer: *T) *T { return pointer; }
fn alias(pointer: *reexport.Future) *owner.Alias { return pointer; }
comptime {
    if (Future != owner.Alias or Future != reexport.Future) @compileError("lost alias identity");
    if (Future == owner.OtherFuture or Future == AnyFuture) @compileError("lost nominal identity");
    if (owner.Nested.Handle != reexport.Nested) @compileError("lost nested identity");
    if (@typeInfo(Future) != .@"opaque") @compileError("opaque reflection tag");
    if (@sizeOf(*Future) != @sizeOf(usize) or @sizeOf(?*Future) != @sizeOf(usize)) @compileError("pointer layout");
}
pub fn main() u8 {
    var backing: u64 = 42;
    const pointer: *Future = @ptrCast(&backing);
    const table = VTable{ .identity = &identity, .maybe = &maybe, .acquire = &acquire };
    const Local = opaque {};
    const local: *Local = @ptrCast(&backing);
    const erased: *anyopaque = local;
    if (@intFromPtr(erased) != @intFromPtr(&backing)) return 5;
    if (table.maybe((null)) != null) return 1;
    if (table.maybe(pointer).? != pointer) return 2;
    const acquired = table.acquire(table.identity(genericIdentity(Future, alias(pointer)))) catch return 3;
    if (@intFromPtr(acquired) != @intFromPtr(&backing)) return 4;
    const restored: *u64 = @ptrCast(@alignCast(acquired));
    return @intCast(restored.*);
}

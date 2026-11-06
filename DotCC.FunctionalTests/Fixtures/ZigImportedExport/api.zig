const abi = @import("abi.zig");
pub const same = zig_imported_add;
pub const identity = zig_imported_identity;
pub const touch = zig_imported_touch;
comptime {
    @export(&same, .{ .name = "zig_imported_add_alias" });
    @export(&zig_imported_options, .{ .name = "zig_imported_options_alias" });
}
export fn zig_imported_add(value: c_int) c_int { return value + 2; }
export fn zig_imported_touch(value: *c_int) callconv(.c) void { value.* += 2; }
export fn zig_imported_options(options: abi.Options) abi.Options {
    var result = options;
    result.emit += 2;
    return result;
}
export fn zig_imported_identity() c_int {
    const direct = &zig_imported_add;
    const alias = &same;
    if (direct != alias) return 3;
    return alias(40);
}
pub fn unused() u32 { @compileError("UNUSED_PUBLIC_EXPORT_CONTROL"); }

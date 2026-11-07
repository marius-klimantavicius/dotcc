const owner = @import("owner.zig");
const Kind = owner.Kind;
const Local = enum(u8) {
    first = 4,
    second = 7,
    pub fn OutType(comptime self: Local) type {
        return switch (self) { .first => u16, .second => bool };
    }
};
const Feature = enum { formatter, search };
fn feature(comptime name: Feature) bool { return @field(owner, @tagName(name)); }
export fn answer() c_int {
    if (comptime !feature(.formatter)) @compileError("incorrect imported true feature");
    if (comptime feature(.search)) @compileError("incorrect imported false feature");
    const A = owner.Kind.tiny.InType();
    const B = Kind.wide.InType();
    const a: A = 7;
    const b: B = 31;
    const items: Kind.tiny.Array(2) = .{ 1, 2 };
    const c: Local.first.OutType() = 3;
    const d: Local.second.OutType() = true;
    if (@typeInfo(A).int.bits != 3 or @typeInfo(B).int.bits != 21) return 1;
    if (!d or @sizeOf(@TypeOf(c)) != 2 or items[0] != 1 or items[1] != 2) return 2;
    return a + b + c + items[0];
}

const Kind = @import("owner.zig").Kind;
export fn answer() c_int {
    const T = Kind.missing.InType();
    const value: T = 42;
    return value;
}

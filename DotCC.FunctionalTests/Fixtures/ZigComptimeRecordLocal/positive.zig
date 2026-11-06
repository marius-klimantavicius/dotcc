const Encoding = union(enum) { scalar, nested: type };
const Options = struct {
    omit: bool = false,
    count: u8 = 7,
    encoding: Encoding = .scalar,
};
fn create(comptime count: u8) Options { return .{ .count = count }; }
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const defaults: Options = .{};
    const first = create(9);
    const second = create(11);
    if (defaults.omit or defaults.count != 7) return 1;
    if (first.omit or first.count != 9 or second.omit or second.count != 11) return 2;
    switch (first.encoding) {
        .scalar => {},
        else => @compileError("unselected record encoding"),
    }
    return 42;
}

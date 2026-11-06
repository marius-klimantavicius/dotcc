const schema = @import("metadata.zig");
const Layout = packed struct(u16) { low: u5, high: u11 };
var effects: u8 = 0;
fn effect() u8 { effects += 1; return 17; }
fn Checked(comptime T: type) type {
    _ = schema.describe(T);
    return T;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const T = Checked(Layout);
    _ = schema.describe(T);
    _ = effect();
    if (T != Layout or effects != 1) return 1;
    return 42;
}

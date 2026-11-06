const std = @import("std");
const Layout = std.builtin.Type.ContainerLayout;
const Auto = struct { value: u8 };
const External = extern struct { value: u8 };
const Packed = packed struct { value: u8 };
fn Box(comptime T: type) type { return struct { value: T }; }
fn inspect(comptime T: type) Layout {
    return @typeInfo(T).@"struct".layout;
}
fn classify(layout: Layout) u8 {
    return switch (layout) { .auto => 1, .@"extern" => 2, .@"packed" => 3 };
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (@sizeOf(Auto) != 1 or @sizeOf(External) != 1 or @sizeOf(Packed) != 1) return 1;
    if (@TypeOf(@typeInfo(Auto).@"struct".layout) != Layout) return 2;
    if (classify(inspect(Auto)) != 1 or classify(inspect(External)) != 2 or classify(inspect(Packed)) != 3) return 3;
    if (classify(inspect(Box(u8))) != 1) return 4;
    var layout = inspect(Auto);
    layout = inspect(External);
    if (classify(layout) != 2) return 5;
    if (@typeInfo(Packed).@"struct".layout != .@"packed") return 6;
    return 42;
}

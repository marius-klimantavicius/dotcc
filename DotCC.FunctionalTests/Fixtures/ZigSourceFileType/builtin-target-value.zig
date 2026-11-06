const builtin = @import("builtin");
const std = @import("std");
fn format(target: anytype) std.Target.ObjectFormat {
    if (@TypeOf(target.ofmt) != std.Target.ObjectFormat) return .coff;
    return target.ofmt;
}
pub export fn targetAnswer() u8 {
    switch (builtin.target.os.tag) {
        .linux => {},
        else => @compileError("the selected Linux target must discard foreign OS bodies"),
    }
    const target = builtin.target;
    if (@TypeOf(builtin.os) != std.Target.Os) return 6;
    if (target.ptrBitWidth() != @bitSizeOf(usize)) return 5;
    if (@TypeOf(target.ofmt) != std.Target.ObjectFormat) return 1;
    if (format(target) != .elf) return 2;
    var copy = target;
    copy.ofmt = .coff;
    if (format(copy) != .coff) return 3;
    if (format(target) != .elf) return 4;
    return 42;
}

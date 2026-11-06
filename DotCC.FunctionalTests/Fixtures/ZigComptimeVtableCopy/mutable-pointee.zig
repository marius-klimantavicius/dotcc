const Cell = struct { value: *const u8 };
const Service = struct { cell: *const Cell };
var runtime: u8 = 17;
const service: Service = .{ .cell = &.{ .value = &runtime } };
const forbidden: u8 = block: {
    const copied = service.cell.*;
    break :block copied.value.*;
};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return forbidden; }

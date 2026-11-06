const std = @import("std");
// This fixture targets the pinned Linux x64 4 KiB page profile.
pub const std_options: std.Options = options: {
    var result: std.Options = .{};
    result.page_size_min = 4096;
    result.page_size_max = 4096;
    break :options result;
};
const Row = struct { value: void, key: *u32, hash: void };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const allocator = std.heap.page_allocator;
    var rows = std.MultiArrayList(Row).initCapacity(allocator, 3) catch return 1;
    defer rows.deinit(allocator);
    var first: u32 = 20;
    var second: u32 = 21;
    var third: u32 = 41;
    rows.appendAssumeCapacity(.{ .value = {}, .key = &first, .hash = {} });
    rows.appendAssumeCapacity(.{ .value = {}, .key = &second, .hash = {} });
    rows.appendAssumeCapacity(.{ .value = {}, .key = &third, .hash = {} });
    rows.swapRemove(0);
    if (rows.len != 2 or rows.items(.key)[0] != &third or rows.items(.key)[1] != &second) return 2;
    rows.items(.key)[0].* += 1;
    if (first != 20 or second != 21 or third != 42) return 3;
    rows.swapRemove(1);
    if (rows.len != 1 or rows.items(.key)[0] != &third) return 4;
    rows.swapRemove(0);
    if (rows.len != 0) return 5;
    return 42;
}

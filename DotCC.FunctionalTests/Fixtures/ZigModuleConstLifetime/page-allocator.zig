const std = @import("std");
// This fixture targets the pinned Linux x64 4 KiB page profile.
pub const std_options: std.Options = options: {
    var result: std.Options = .{};
    result.page_size_min = 4096;
    result.page_size_max = 4096;
    break :options result;
};
fn obtain() std.mem.Allocator { return std.heap.page_allocator; }
fn clobber(seed: u64) u64 {
    var values: [256]u64 = undefined;
    for (&values, 0..) |*value, index| value.* = seed + index;
    return values[255];
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const allocator = obtain();
    if (allocator.vtable != &std.heap.PageAllocator.vtable) return 1;
    if (clobber(4) != 259 or obtain().vtable != allocator.vtable) return 2;
    const alignment = std.mem.Alignment.fromByteUnits(4096);
    const pointer = allocator.rawAlloc(8192, alignment, 0) orelse return 3;
    defer allocator.rawFree(pointer[0..8192], alignment, 0);
    if (@intFromPtr(pointer) & 4095 != 0) return 4;
    pointer[0] = 20;
    pointer[8191] = 22;
    return pointer[0] + pointer[8191];
}

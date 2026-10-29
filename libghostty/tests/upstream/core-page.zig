//! Test-only zeroed page allocation service; page algorithms stay upstream.
const std = @import("std");
extern fn dotcc_upstream_host_alloc(usize, u8) ?*anyopaque;
extern fn dotcc_upstream_host_free(?*anyopaque, usize, u8) void;

pub fn alloc(len: usize) error{OutOfMemory}![]align(std.heap.page_size_min) u8 {
    const ptr = dotcc_upstream_host_alloc(len, @intCast(std.math.log2_int(usize, std.heap.page_size_min))) orelse
        return error.OutOfMemory;
    const aligned: [*]align(std.heap.page_size_min) u8 = @ptrCast(@alignCast(ptr));
    @memset(aligned[0..len], 0);
    return aligned[0..len];
}
pub fn free(memory: []align(std.heap.page_size_min) u8) void {
    dotcc_upstream_host_free(@ptrCast(memory.ptr), memory.len, @intCast(std.math.log2_int(usize, std.heap.page_size_min)));
}

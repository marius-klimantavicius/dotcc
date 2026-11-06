const std = @import("std");
const c = std.c;
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer(path: [*:0]const u8) callconv(.c) u8 {
    const fd = c.openat64(-100, path, .{ .ACCMODE = .RDWR, .CREAT = true, .TRUNC = true }, @as(c_uint, 0o600));
    if (fd < 0) return 1;
    if (c.write(fd, "abcdef", 6) != 6) return 2;
    if (c.lseek64(fd, 2, 0) != 2) return 3;
    var buffer: [3]u8 = undefined;
    const vectors = [_]c.iovec{.{ .base = &buffer, .len = buffer.len }};
    if (c.preadv64(fd, &vectors, 1, 1) != 3) return 4;
    if (buffer[0] != 'b' or buffer[1] != 'c' or buffer[2] != 'd') return 5;
    if (c.lseek64(fd, 0, 1) != 2) return 6;
    if (c.readv(fd, &vectors, 1) != 3 or buffer[0] != 'c' or buffer[2] != 'e') return 20;
    if (c.readv(fd, &vectors, 0xffffffff) != -1 or c._errno().* != 22) return 21;
    if (c.close(fd) != 0) return 7;
    if (c.unlinkat(-100, path, 0) != 0) return 8;
    if (c.lseek64(fd, 0, 0) != -1 or c._errno().* != 9) return 9;
    const allocation = c.malloc(17) orelse return 10;
    const usable = c.malloc_usable_size(allocation);
    if (usable < 17) return 11;
    const mallocSize = selected: {
        if (@TypeOf(c.malloc_usable_size) != void) break :selected c.malloc_usable_size;
        return 18;
    };
    if (mallocSize(allocation) != usable) return 19;
    const heap: [*]u8 = @ptrCast(allocation);
    heap[usable - 1] = 67;
    if (heap[usable - 1] != 67) return 12;
    c.free(allocation);
    const mapping = c.mmap64(null, 4096, .{ .READ = true, .WRITE = true }, .{ .TYPE = .PRIVATE, .ANONYMOUS = true }, -1, 0);
    if (@intFromPtr(mapping) == 0xffffffffffffffff) return 13;
    if (c.madvise(@alignCast(mapping), 4096, 0) != 0) return 22;
    if (c.madvise(@alignCast(mapping), 4096, 0xffffffff) != -1 or c._errno().* != 22) return 23;
    const bytes: [*]u8 = @ptrCast(mapping);
    bytes[0] = 42;
    bytes[4095] = 99;
    const grown = c.mremap(@as(?*align(4096) const anyopaque, @alignCast(mapping)), 4096, 8192, .{ .MAYMOVE = true });
    if (@intFromPtr(grown) == 0xffffffffffffffff) return 14;
    const expanded: [*]u8 = @ptrCast(grown);
    if (expanded[0] != 42 or expanded[4095] != 99) return 15;
    expanded[8191] = 71;
    if (expanded[8191] != 71) return 16;
    if (c.munmap(@alignCast(grown), 8192) != 0) return 17;
    return 42;
}

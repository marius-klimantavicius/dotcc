//! Test-only runner. Every test function and std.testing assertion remains upstream.
//! The host provides allocation and reporting, never terminal behavior.
const std = @import("std");
const builtin = @import("builtin");
const selection = @import("selection.zig");
var selected_indices: [selection.names.len]usize = undefined;
var selection_ready = false;
fn initializeSelection() void {
    if (selection_ready) return;
    var count: usize = 0;
    for (builtin.test_functions, 0..) |entry, index| {
        for (selection.names) |name| {
            if (std.mem.eql(u8, entry.name, name)) {
                selected_indices[count] = index;
                count += 1;
                break;
            }
        }
    }
    if (count != selection.names.len) @panic("upstream core inventory mismatch");
    selection_ready = true;
}
fn selectedTest(index: usize) std.builtin.TestFn {
    initializeSelection();
    return builtin.test_functions[selected_indices[index]];
}

extern fn dotcc_upstream_host_alloc(usize, u8) ?*anyopaque;
extern fn dotcc_upstream_host_free(?*anyopaque, usize, u8) void;
extern fn dotcc_upstream_host_report([*]const u8, usize, c_int, [*]const u8, usize) void;
extern fn dotcc_upstream_host_finish(usize) c_int;
extern fn dotcc_upstream_host_log([*]const u8, usize) void;
extern fn dotcc_upstream_io_live() usize;

pub const std_options: std.Options = .{
    .page_size_min = 4096,
    .page_size_max = 4096,
    .allow_stack_tracing = false,
    .logFn = log,
};
// A test assertion still returns its original error if diagnostic output fails.
// No filesystem/process/thread services are claimed by this test runner.
pub const std_options_debug_io: std.Io = .failing;
pub const dotcc_upstream_test_io: std.Io = @import("core-io.zig").io;
pub const dotcc_upstream_handle_type = c_int;
pub const dotcc_upstream_page_alloc = @import("core-page.zig");
pub const os = struct {
    pub const PATH_MAX = 4096;
    pub const NAME_MAX = 255;
    pub const heap = struct {
        pub const page_allocator = backing;
    };
};
pub fn std_options_cwd() std.Io.Dir {
    return .{ .handle = -100 };
}
pub const panic = std.debug.FullPanic(panicImpl);
pub const debug = struct {
    pub fn getDebugInfoAllocator() std.mem.Allocator {
        return backing;
    }
};
var log_errors: usize = 0;
fn log(comptime level: std.log.Level, comptime scope: @TypeOf(.enum_literal), comptime fmt: []const u8, args: anytype) void {
    _ = scope;
    if (@intFromEnum(level) > @intFromEnum(std.testing.log_level)) return;
    if (level == .err) log_errors += 1;
    var buffer: [4096]u8 = undefined;
    const text = std.mem.print(&buffer, fmt, args) catch "test diagnostic exceeds buffer";
    dotcc_upstream_host_log(text.ptr, text.len);
}

fn panicImpl(msg: []const u8, ra: ?usize) noreturn {
    _ = ra;
    dotcc_upstream_host_report("panic", 5, 4, msg.ptr, msg.len);
    @trap();
}

fn alloc(ctx: *anyopaque, len: usize, alignment: std.mem.Alignment, ra: usize) ?[*]u8 {
    _ = ctx; _ = ra;
    return @ptrCast(dotcc_upstream_host_alloc(len, @backingInt(alignment)));
}
fn resize(ctx: *anyopaque, mem: []u8, alignment: std.mem.Alignment, len: usize, ra: usize) bool {
    _ = ctx; _ = mem; _ = alignment; _ = len; _ = ra;
    return false;
}
fn remap(ctx: *anyopaque, mem: []u8, alignment: std.mem.Alignment, len: usize, ra: usize) ?[*]u8 {
    _ = ctx; _ = mem; _ = alignment; _ = len; _ = ra;
    return null;
}
fn free(ctx: *anyopaque, mem: []u8, alignment: std.mem.Alignment, ra: usize) void {
    _ = ctx; _ = ra;
    dotcc_upstream_host_free(@ptrCast(mem.ptr), mem.len, @backingInt(alignment));
}
const vtable: std.mem.Allocator.VTable = .{ .alloc = alloc, .resize = resize, .remap = remap, .free = free };
var context: u8 = 0;
const backing: std.mem.Allocator = .{ .ptr = &context, .vtable = &vtable };

export fn dotcc_upstream_count() usize {
    initializeSelection();
    return selection.names.len;
}

export fn dotcc_upstream_name(index: usize, len: *usize) [*]const u8 {
    const name = selectedTest(index).name;
    len.* = name.len;
    return name.ptr;
}
export fn dotcc_upstream_run(index: usize) c_int {
    std.testing.random_seed = 0;
    std.testing.log_level = .err;
    log_errors = 0;
    std.testing.allocator_instance = .init(backing, .{
        .stack_trace_frames = 0,
        .canary = 0xc3a701ba,
        .check_write_after_free = true,
    });
    std.testing.environ = .empty;
    if (builtin.os.tag != .freestanding) {
        std.testing.io_instance = .init(std.testing.allocator, .{ .environ = .empty });
    }
    var status: c_int = 0;
    var err_name: []const u8 = "";
    selectedTest(index).func() catch |err| {
        status = if (err == error.SkipZigTest) 1 else 2;
        err_name = @errorName(err);
    };
    if (builtin.os.tag != .freestanding) std.testing.io_instance.deinit();
    if (std.testing.allocator_instance.deinit() != 0) status = 3;
    if (dotcc_upstream_io_live() != 0) status = 6;
    if (log_errors != 0 and status == 0) status = 5;
    const name = selectedTest(index).name;
    dotcc_upstream_host_report(name.ptr, name.len, status, err_name.ptr, err_name.len);
    return status;
}

pub const main = if (builtin.os.tag == .freestanding) cEntry else nativeEntry;
fn cEntry() callconv(.c) c_int {
    return runAll();
}
fn nativeEntry() u8 {
    return @intCast(runAll());
}
fn runAll() c_int {
    const count = dotcc_upstream_count();
    for (0..count) |index| {
        _ = dotcc_upstream_run(index);
    }
    return dotcc_upstream_host_finish(count);
}

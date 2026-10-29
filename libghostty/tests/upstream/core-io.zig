//! Read-only fixture Io adapter. All parsing/assertions remain upstream.
const std = @import("std");
const Io = std.Io;
const Dir = Io.Dir;
const File = Io.File;

extern fn dotcc_upstream_dir_open(c_int, [*]const u8, usize, *c_int) c_int;
extern fn dotcc_upstream_dir_next(c_int, [*]u8, usize, *usize, *c_int) c_int;
extern fn dotcc_upstream_file_open(c_int, [*]const u8, usize, *c_int) c_int;
extern fn dotcc_upstream_file_size(c_int, *u64) c_int;
extern fn dotcc_upstream_file_read_at(c_int, u64, [*]u8, usize, *usize) c_int;
extern fn dotcc_upstream_dir_close(c_int) void;
extern fn dotcc_upstream_file_close(c_int) void;
extern fn dotcc_upstream_random_secure([*]u8, usize) c_int;

pub const io: Io = .{ .userdata = null, .vtable = &vtable };
const vtable: Io.VTable = table: {
    var result = Io.failing.vtable.*;
    result.dirOpenDir = openDir;
    result.dirRead = readDir;
    result.dirClose = closeDirs;
    result.dirOpenFile = openFile;
    result.fileStat = statFile;
    result.fileLength = lengthFile;
    result.fileReadPositional = readFile;
    result.fileClose = closeFiles;
    result.randomSecure = randomSecure;
    break :table result;
};

fn openDir(_: ?*anyopaque, base: Dir, path: []const u8, options: Dir.OpenOptions) Dir.OpenError!Dir {
    _ = options;
    var handle: c_int = undefined;
    const status = dotcc_upstream_dir_open(base.handle, path.ptr, path.len, &handle);
    switch (status) {
        0 => return .{ .handle = handle },
        -2 => return error.FileNotFound,
        -13 => return error.AccessDenied,
        -20 => return error.NotDir,
        -36 => return error.NameTooLong,
        else => return error.SystemResources,
    }
}
fn readDir(_: ?*anyopaque, reader: *Dir.Reader, entries: []Dir.Entry) Dir.Reader.Error!usize {
    if (entries.len == 0 or reader.state == .finished) return 0;
    if (reader.state == .reset and reader.index != 0) return error.SystemResources;
    var len: usize = 0;
    var kind: c_int = 0;
    const status = dotcc_upstream_dir_next(reader.dir.handle, reader.buffer.ptr, reader.buffer.len, &len, &kind);
    if (status == 1) {
        reader.state = .finished;
        return 0;
    }
    if (status != 0 or len > reader.buffer.len) return error.SystemResources;
    reader.state = .reading;
    reader.index += 1;
    entries[0] = .{
        .name = reader.buffer[0..len],
        .kind = switch (kind) { 1 => .file, 2 => .directory, else => .unknown },
        .inode = std.mem.zeroes(File.INode),
    };
    return 1;
}
fn closeDirs(_: ?*anyopaque, dirs: []const Dir) void {
    for (dirs) |dir| dotcc_upstream_dir_close(dir.handle);
}
fn openFile(_: ?*anyopaque, dir: Dir, path: []const u8, options: Dir.OpenFileOptions) File.OpenError!File {
    if (options.mode != .read_only or options.path_only) return error.AccessDenied;
    var handle: c_int = undefined;
    const status = dotcc_upstream_file_open(dir.handle, path.ptr, path.len, &handle);
    switch (status) {
        0 => return .{ .handle = handle, .flags = .{ .nonblocking = false } },
        -2 => return error.FileNotFound,
        -13 => return error.AccessDenied,
        -20 => return error.NotDir,
        -36 => return error.NameTooLong,
        else => return error.SystemResources,
    }
}
fn lengthFile(_: ?*anyopaque, file: File) File.LengthError!u64 {
    var len: u64 = 0;
    if (dotcc_upstream_file_size(file.handle, &len) != 0) return error.SystemResources;
    return len;
}
fn statFile(userdata: ?*anyopaque, file: File) File.StatError!File.Stat {
    const len = lengthFile(userdata, file) catch return error.SystemResources;
    var result: File.Stat = std.mem.zeroes(File.Stat);
    result.size = len;
    result.kind = .file;
    result.block_size = 1;
    return result;
}
fn readFile(_: ?*anyopaque, file: File, buffers: []const []u8, offset: u64) File.ReadPositionalError!usize {
    var total: usize = 0;
    for (buffers) |buffer| {
        if (buffer.len == 0) continue;
        var read: usize = 0;
        if (dotcc_upstream_file_read_at(file.handle, offset + total, buffer.ptr, buffer.len, &read) != 0)
            return error.InputOutput;
        if (read > buffer.len) return error.InputOutput;
        total += read;
        if (read != buffer.len) break;
    }
    return total;
}
fn closeFiles(_: ?*anyopaque, files: []const File) void {
    for (files) |file| dotcc_upstream_file_close(file.handle);
}
fn randomSecure(_: ?*anyopaque, bytes: []u8) Io.RandomSecureError!void {
    if (dotcc_upstream_random_secure(bytes.ptr, bytes.len) != 0) return error.EntropyUnavailable;
}

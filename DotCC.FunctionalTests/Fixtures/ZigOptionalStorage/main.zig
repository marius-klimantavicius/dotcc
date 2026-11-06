const VTable = struct { read: *const fn (*anyopaque) u64 };
const Allocator = struct { ptr: *anyopaque, vtable: *const VTable };
const Record = struct { before: u64, alloc: ?Allocator, after: u64 };
const MaybeArray = ?[3]u32;
const Scalar = ?u64;

fn read(context: *anyopaque) u64 {
    const value: *u64 = @ptrCast(@alignCast(context));
    return value.*;
}

pub fn main() u8 {
    if (@sizeOf(Allocator) != 16 or @sizeOf(?Allocator) != 24 or @alignOf(?Allocator) != 8) return 1;
    if (@sizeOf(Scalar) != 16 or @alignOf(Scalar) != 8) return 2;
    if (@sizeOf(MaybeArray) != 16 or @alignOf(MaybeArray) != 4) return 3;
    if (@sizeOf(?*Allocator) != 8 or @alignOf(?*Allocator) != 8) return 4;
    if (@sizeOf(Record) != 40 or @offsetOf(Record, "alloc") != 8 or @offsetOf(Record, "after") != 32) return 5;

    // The bound is folded by the layout model; typed stores and indexing use
    // the emitted CLR extent. A disagreement corrupts the guards.
    var storage: [@sizeOf(Record) + 16]u8 align(8) = undefined;
    @memset(&storage, 0xa5);
    const record: *Record = @ptrCast(@alignCast(&storage[8]));
    var answer: u64 = 42;
    const table: VTable = .{ .read = &read };
    const allocator: Allocator = .{ .ptr = &answer, .vtable = &table };
    record.* = .{ .before = 19, .alloc = allocator, .after = 23 };
    const copy = record.alloc;
    record.alloc = null;
    if (record.alloc != null or record.before + record.after != 42) return 6;
    const restored = copy orelse return 7;
    if (restored.vtable.read(restored.ptr) != 42) return 8;
    record.alloc = restored;
    if (record.alloc.?.vtable.read(record.alloc.?.ptr) != 42) return 9;
    if (@intFromPtr(&record.after) - @intFromPtr(record) != @offsetOf(Record, "after")) return 10;
    if (storage[0] != 0xa5 or storage[7] != 0xa5 or storage[@sizeOf(Record) + 8] != 0xa5 or storage[@sizeOf(Record) + 15] != 0xa5) return 11;

    var scalar_storage: [@sizeOf(Scalar) * 2 + 16]u8 align(8) = undefined;
    @memset(&scalar_storage, 0xa5);
    const scalars: [*]Scalar = @ptrCast(@alignCast(&scalar_storage[8]));
    scalars[0] = 20;
    scalars[1] = 22;
    const first = scalars[0] orelse return 12;
    const second = scalars[1] orelse return 13;
    if (first + second != 42) return 14;
    if (@intFromPtr(&scalars[1]) - @intFromPtr(&scalars[0]) != @sizeOf(Scalar)) return 15;
    if (scalar_storage[7] != 0xa5 or scalar_storage[@sizeOf(Scalar) * 2 + 8] != 0xa5) return 16;

    var array: MaybeArray = [3]u32{ 10, 20, 12 };
    const values = array orelse return 17;
    if (values[0] + values[1] + values[2] != 42) return 18;
    array = null;
    if (array != null) return 19;
    return 42;
}

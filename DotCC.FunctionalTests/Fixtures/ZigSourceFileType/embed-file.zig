const provider = @import("embed/provider.zig");
const own = @embedFile("embed/res/bytes.bin");
const Holder = extern struct {
    data: @typeInfo(@TypeOf(provider.bytes)).pointer.child,
    next: u8,
};

fn checksum(bytes: []const u8) u32 {
    var sum: u32 = 0;
    for (bytes) |b| sum += b;
    return sum;
}

fn byteArrayType(value: anytype) bool {
    return @TypeOf(value) == [14:0]u8;
}

pub fn main() u8 {
    const bytes = provider.bytes;
    const Ptr = @TypeOf(bytes);
    const Array = @typeInfo(Ptr).pointer.child;
    if (@typeInfo(Ptr) != .pointer) return 1;
    if (!@typeInfo(Ptr).pointer.attrs.@"const") return 2;
    if (@typeInfo(Ptr).pointer.size != .one) return 3;
    if (@typeInfo(Ptr).pointer.sentinel()) |_| return 3;
    const length = @typeInfo(Array).array.len;
    if (length != 14) return 4;
    if (@typeInfo(Array).array.sentinel()) |sentinel| {
        if (sentinel != 0) return 5;
    } else return 5;
    if (@sizeOf(Ptr) != @sizeOf(usize)) return 6;
    if (@sizeOf(Array) != 15) return 7;
    if (@sizeOf(@TypeOf(bytes.*)) != 15) return 8;
    if (bytes.len != 14 or own.len != 14) return 9;
    if (provider.empty.len != 0 or provider.empty[0] != 0) return 10;
    if (@sizeOf(@TypeOf(provider.empty.*)) != 1) return 11;
    if (bytes[14] != 0 or bytes[0] != 0 or bytes[2] != 128 or bytes[3] != 255) return 12;
    if (bytes[4] != 195 or bytes[5] != 40 or bytes[6] != 13 or bytes[7] != 10) return 13;
    if (bytes[8] != 239 or bytes[9] != 187 or bytes[10] != 191 or bytes[13] != 0) return 14;
    if (checksum(bytes) != 1511 or checksum(own) != 1511) return 15;
    const folded = comptime checksum(provider.bytes);
    if (folded != 1511) return 16;
    const view: []const u8 = bytes;
    if (view.len != 14 or view.ptr[view.len] != 0) return 17;
    var copy = bytes.*;
    copy[0] = 42;
    if (copy.len != 14 or copy[14] != 0 or bytes[0] != 0 or copy[0] != 42) return 18;
    if (provider.bom.len != 7 or provider.bom[0] != 239 or provider.bom[6] != 0) return 19;
    var holder = Holder{ .data = bytes.*, .next = 42 };
    holder.data[0] = 33;
    if (@sizeOf(Holder) != 16 or @offsetOf(Holder, "next") != 15) return 20;
    if (holder.data[14] != 0 or holder.next != 42 or bytes[0] != 0) return 21;
    if (Ptr != *const [14:0]u8) return 22;
    if (Ptr == *const [14]u8) return 23;
    if (Array != [14:0]u8 or @TypeOf(bytes.*) != [14:0]u8) return 23;
    if (!byteArrayType(bytes.*)) return 23;
    var ordinary: [3:9]u8 = .{ 1, 2, 3 };
    const ordinary_copy = ordinary;
    ordinary[0] = 7;
    if (@sizeOf(@TypeOf(ordinary)) != 4 or ordinary_copy.len != 3) return 24;
    if (ordinary_copy[0] != 1 or ordinary_copy[3] != 9) return 25;
    return 42;
}

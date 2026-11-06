fn forward(value: []const u16) []const u16 { return value; }
fn inspect(value: anytype) u16 {
    const info = @typeInfo(@TypeOf(value)).pointer;
    const pointer = info.sentinel_ptr;
    if (pointer == null) return 10;
    const scalar: *const u16 = @ptrCast(@alignCast(pointer.?));
    return scalar.*;
}
pub fn main() u8 {
    const buffer = [2:7]u16{ 1, 2 };
    const plain: []const u16 = &buffer;
    const sent: [:7]const u16 = &buffer;
    const Box = struct { items: ?[]const u16 };
    const box: Box = .{ .items = plain };
    if (box.items) |items| { if (inspect(items) != 10) return 3; }
    const selected = box.items orelse return 4;
    if (inspect(selected) != 10) return 5;
    if (inspect(plain) != 10 or inspect(forward(plain)) != 10 or inspect(sent) != 7) return 1;
    const other = [2:9]u16{ 3, 4 };
    const other_slice: [:9]const u16 = &other;
    if (inspect(other_slice) != 9) return 2;
    const Plain = struct { items: [1][*]const u16 };
    const Marked = struct { items: [1][*:7]const u16 };
    const Nested = struct { items: []const [*]const u16 };
    const pointers = [_][*]const u16{ &buffer };
    const plain_box: Plain = .{ .items = pointers };
    const marked_box: Marked = .{ .items = .{&buffer} };
    const nested: Nested = .{ .items = &pointers };
    if (inspect(plain_box.items[0]) != 10 or inspect(marked_box.items[0]) != 7) return 6;
    const selected_plain = nested.items[0];
    if (inspect(selected_plain) != 10 or inspect(nested.items[0][0..2]) != 10) return 7;
    var values = [2]u16{ 17, 25 };
    const Requests = struct { items: []const [*]u16 };
    const request_pointers = [_][*]u16{ &values };
    const requests: Requests = .{ .items = &request_pointers };
    const range = requests.items[0][0..2];
    if (inspect(range) != 10 or range[0] + range[1] != 42) return 8;
    const info = @typeInfo([2:16]u16).array;
    const scalar: *const u16 = @ptrCast(@alignCast(info.sentinel_ptr.?));
    return @intCast(inspect(plain) + inspect(sent) + inspect(other_slice) + scalar.*);
}

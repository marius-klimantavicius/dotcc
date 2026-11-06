const Counter = struct { count: usize = 0 };
fn grow(counter: *Counter, amount: usize) void { counter.count += amount; }
const bytes: [:0]const u8 = data: {
    var counter: Counter = .{};
    grow(&counter, 2);
    grow(&counter, 3);
    var buffer: [counter.count:0]u8 = @splat('a');
    var i: usize = 0;
    while (i < counter.count) : (i += 1) buffer[i] = @intCast('a' + i);
    const final = buffer;
    break :data final[0..counter.count :0];
};
const cleaned: []const u8 = data: {
    var counter: Counter = .{};
    grow(&counter, 1);
    const ignored: usize = inner: {
        defer grow(&counter, 1);
        break :inner 2;
    };
    _ = ignored;
    const buffer: [counter.count]u8 = @splat('x');
    const final = buffer;
    break :data final[0..counter.count];
};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (bytes.len != 5 or bytes[0] != 'a' or bytes[4] != 'e' or bytes.ptr[5] != 0) return 1;
    if (cleaned.len != 2 or cleaned[0] != 'x' or cleaned[1] != 'x') return 2;
    return 42;
}

const Counter = struct { count: usize = 0 };
fn grow(counter: *Counter) void { counter.count += 1; }
const bytes: []const u8 = data: {
    var counter: Counter = .{};
    grow(&counter);
    if (counter.count == 1) @compileError("reached counter validation");
    break :data "unused";
};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 { return @intCast(bytes.len); }

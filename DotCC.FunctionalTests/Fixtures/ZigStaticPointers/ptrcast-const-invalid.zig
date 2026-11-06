const Record = extern struct { value: usize, marker: u32 };
const storage: [16]u8 = @splat(0);
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const bytes = @as([*]const u8, @ptrCast(&storage))[0..@sizeOf(Record)];
    const mutable: *Record = @ptrCast(@alignCast(bytes));
    return @intCast(mutable.value);
}

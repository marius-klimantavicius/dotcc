const Record = extern struct { value: usize, marker: u32 };
var storage: [4]usize = .{ 0, 0, 0, 0 };
var calls: u32 = 0;
var missing: bool = false;
fn address() ?[*]u8 {
    calls += 1;
    if (missing) return null;
    return @ptrCast(&storage);
}
fn record() error{Missing}!*Record {
    return @ptrCast(@alignCast(address() orelse return error.Missing));
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    calls = 0;
    missing = false;
    const first = record() catch return 1;
    if (calls != 1) return 2;
    first.value = 17;
    first.marker = 42;
    if (storage[0] != 17) return 3;
    const bytes = @as([*]u8, @ptrCast(&storage))[0..@sizeOf(Record)];
    const second: *Record = @ptrCast(@alignCast(bytes));
    if (second != first or second.marker != 42) return 4;
    calls = 0;
    missing = true;
    var caught = false;
    _ = record() catch |err| {
        if (err != error.Missing or calls != 1) return 5;
        caught = true;
    };
    return if (caught) 42 else 6;
}

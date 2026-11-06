const Mutex = enum(u8) { unlocked, locked };
const Other = enum(u8) { unlocked, locked };
var mutex: Mutex = .unlocked;
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    _ = @cmpxchgStrong(Mutex, &mutex, Other.unlocked, .locked, .acquire, .monotonic);
    return 42;
}

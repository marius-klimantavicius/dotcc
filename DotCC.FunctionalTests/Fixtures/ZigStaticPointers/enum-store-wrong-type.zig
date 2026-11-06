const Mutex = enum(u8) { unlocked, locked };
const Other = enum(u8) { unlocked, locked };
var mutex: Mutex = .unlocked;
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    @atomicStore(Mutex, &mutex, Other.locked, .release);
    return 42;
}

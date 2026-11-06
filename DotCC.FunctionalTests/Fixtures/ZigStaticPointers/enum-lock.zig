const Mutex = enum(u8) { unlocked, locked };
var mutex: Mutex = .unlocked;
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (@cmpxchgStrong(Mutex, &mutex, .unlocked, .locked, .acquire, .monotonic) != null) return 1;
    if (@atomicLoad(Mutex, &mutex, .unordered) != .locked) return 2;
    const busy = @cmpxchgWeak(Mutex, &mutex, .unlocked, .locked, .monotonic, .monotonic);
    if (busy == null or busy.? != .locked or mutex != .locked) return 3;
    @atomicStore(Mutex, &mutex, .unlocked, .release);
    const zero = @cmpxchgStrong(Mutex, &mutex, .locked, .locked, .monotonic, .monotonic);
    if (zero == null or zero.? != .unlocked) return 4;
    if (@atomicLoad(Mutex, &mutex, .acquire) != .unlocked) return 5;
    return 42;
}

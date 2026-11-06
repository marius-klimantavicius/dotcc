const Holder = struct {
    pub const Failure = error{OutOfMemory};
    pub const Combined = Failure || error{Denied};
};
fn run(fail: bool) Holder.Failure!u8 {
    if (fail) return Holder.Failure.OutOfMemory;
    return 42;
}
pub fn main() u8 {
    const oom = Holder.Failure.OutOfMemory;
    const denied = Holder.Combined.Denied;
    if (oom != error.OutOfMemory or denied != error.Denied) return 1;
    if (run(true)) |_| return 2 else |err| { if (err != error.OutOfMemory) return 3; }
    return run(false) catch return 4;
}

var trace: u32 = 0;
fn mark(n: u32) void { trace = trace * 10 + n; }
fn dispatch(n: u8) void {
    outer: {
        defer mark(4);
        switch (n) {
            0 => { mark(1); break :outer {}; },
            1 => {
                inner: {
                    defer mark(3);
                    mark(2);
                    break :inner mark(1);
                }
                break :outer {};
            },
            else => {},
        }
        mark(9);
    }
    mark(5);
}
pub fn main() u8 {
    dispatch(0);
    if (trace != 145) return 1;
    trace = 0;
    dispatch(1);
    if (trace != 21345) return 2;
    trace = 0;
    dispatch(2);
    if (trace != 945) return 3;
    return 42;
}

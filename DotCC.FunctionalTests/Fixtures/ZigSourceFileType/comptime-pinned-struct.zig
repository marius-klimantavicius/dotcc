const Inner = struct { value: u8 };
const Outer = struct { inner: *const Inner };
inline fn make() Outer {
    comptime {
        return .{ .inner = &.{ .value = 42 } };
    }
}
const outer = make();
fn check() u8 {
    if (outer.inner.value != 42) return 1;
    const copy = outer.inner.*;
    return copy.value;
}
pub fn main() u8 {
    const answer = comptime check();
    return answer;
}

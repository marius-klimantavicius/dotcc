const facade = @import("facade.zig");
const Result = facade.Result;
const DirectResult = facade.api.Result;

fn check(value: Result) u8 {
    return if (value == .success) 20 else if (value == .invalid_value) 22 else 1;
}

pub fn main() u8 {
    const ok: DirectResult = facade.result(true);
    const bad: Result = facade.api.result(false);
    return check(ok) + check(bad);
}

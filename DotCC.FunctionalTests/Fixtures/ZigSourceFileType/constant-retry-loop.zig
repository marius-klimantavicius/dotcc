const enabled = true;
fn retry(value: u8) u8 {
    if (enabled) {
        var count: u8 = 0;
        while (true) {
            count += 1;
            switch (count) {
                1 => continue,
                else => return value,
            }
        }
    }
    @compileError("comptime-dead fallback after retry loop");
}
fn breaks() u8 {
    while (true) { break; }
    return 10;
}
fn nestedBreak() u8 {
    while (true) {
        while (true) { break; }
        return 11;
    }
}
fn switchBreak(value: u8) u8 {
    while (true) {
        switch (value) { 1 => break, else => return 1 }
    }
    return 12;
}
fn runtimeCondition(value: bool) u8 {
    while (value) { return 1; }
    return 9;
}
pub fn main() u8 {
    return retry(breaks() + nestedBreak() + switchBreak(1) + runtimeCondition(false));
}

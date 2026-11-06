var tests: u32 = 0;
var fallbacks: u32 = 0;
var trace: u64 = 0;
var gates: u32 = 0;
fn gate() bool {
    gates += 1;
    return gates < 3;
}
fn bound() ?i128 {
    tests += 1;
    trace = trace * 10 + 1;
    if (tests == 2) return null;
    return 3;
}
fn fallback() i128 {
    fallbacks += 1;
    trace = trace * 10 + 2;
    return 3;
}
fn empty_test() ?i128 {
    tests += 1;
    if (tests <= 3) return 1;
    return null;
}
pub fn main() u8 {
    var index: i128 = 0;
    var bodies: u32 = 0;
    while (index < (bound() orelse fallback())) {
        index += 1;
        trace = trace * 10 + 3;
        if (index == 2) continue;
        bodies += 1;
    } else {
        trace = trace * 10 + 4;
    }
    if (tests != 4 or fallbacks != 1 or bodies != 2 or trace != 131231314) return 1;
    tests = 0;
    fallbacks = 0;
    while ((empty_test() orelse 0) != 0) {}
    if (tests != 4 or fallbacks != 0) return 2;
    tests = 0;
    index = 0;
    var else_runs: u32 = 0;
    while (index < (bound() orelse fallback())) {
        index += 1;
        if (index == 2) break;
    } else {
        else_runs += 1;
    }
    if (tests != 2 or index != 2 or else_runs != 0) return 3;
    tests = 0;
    while (gate() and ((empty_test() orelse 0) != 0)) {}
    if (gates != 3 or tests != 2) return 4;
    return 42;
}

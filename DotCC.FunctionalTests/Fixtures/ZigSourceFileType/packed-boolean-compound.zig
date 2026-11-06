const Flags = packed struct(u8) { prefix: u3 = 5, dirty: bool = false, suffix: u4 = 11 };
fn pointer(flags: *Flags, trace: *u8) *Flags { trace.* = trace.* * 10 + 1; return flags; }
fn operand(input: bool, trace: *u8) bool { trace.* = trace.* * 10 + 2; return input; }
pub fn main() u8 {
    for ([_]bool{ false, true }) |a| {
        for ([_]bool{ false, true }) |b| {
            var flags: Flags = .{ .dirty = a };
            var trace: u8 = 0;
            pointer(&flags, &trace).dirty |= operand(b, &trace);
            if (flags.dirty != (a or b) or trace != 12 or flags.prefix != 5 or flags.suffix != 11) return 1;
            flags.dirty = a;
            flags.dirty &= b;
            if (flags.dirty != (a and b) or flags.prefix != 5 or flags.suffix != 11) return 2;
            flags.dirty = a;
            flags.dirty ^= b;
            if (flags.dirty != (a != b) or flags.prefix != 5 or flags.suffix != 11) return 3;
        }
    }
    return 42;
}

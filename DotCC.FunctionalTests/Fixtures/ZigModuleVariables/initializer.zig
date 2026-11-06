const module = @import("initializers.zig");
pub fn main() u8 {
    if (module.Reader.first() != 16) return 1;
    if (module.read() != 16) return 2;
    const pointer_slot = module.pointer();
    pointer_slot.*.* += 5;
    if (module.read() != 21) return 3;
    return @intCast(module.read() * 2);
}

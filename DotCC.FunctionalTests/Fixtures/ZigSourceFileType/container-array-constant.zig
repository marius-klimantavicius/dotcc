const provider = @import("palette-defaults.zig");
pub fn main() u8 {
    var first: provider.Dynamic = .default;
    const second: provider.Dynamic = .default;
    if (first.current[0].r != 1 or first.current[3].b != 33) return 1;
    if (second.current[1].g != 12 or second.mask != 0) return 2;
    if (first.original != &provider.default or first.original != second.original) return 3;
    first.current[1].g = 42;
    if (second.current[1].g != 12 or first.original[1].g != 12) return 4;
    var copy = first;
    copy.current[1].g = 7;
    if (first.current[1].g != 42 or copy.current[1].g != 7) return 5;
    const again: provider.Dynamic = .default;
    if (again.current[1].g != 12) return 6;
    return first.current[1].g;
}

const provider = @import("method-alias-provider.zig");
fn pick(count: *u8) provider.Kind { count.* += 1; return .first; }
pub fn main() u8 {
    const isFirst: u8 = 0;
    _ = isFirst;
    const value: provider.Kind = .first;
    if (!value.isOLD() or provider.Kind.second.isOLD()) return 1;
    if (!provider.Kind.isOLD(.first)) return 2;
    var count: u8 = 0;
    if (!pick(&count).isOLD() or count != 1) return 3;
    return 42;
}

const Size = packed struct(usize) {
    resizing: bool,
    size: u63 = 0,
};
var effects: u32 = 0;
var word: Size = .{ .resizing = false, .size = 42 };
fn address() *Size { effects = effects * 10 + 1; return &word; }
fn operand() Size { effects = effects * 10 + 2; return .{ .resizing = true }; }
pub fn main() u8 {
    if (@sizeOf(Size) != 8 or @alignOf(Size) != 8) return 1;
    const initial = @atomicLoad(Size, &word, .monotonic);
    if (initial.resizing or initial.size != 42) return 2;
    const old = @atomicRmw(Size, address(), .Or, operand(), .acquire);
    if (old.resizing or old.size != 42 or effects != 12) return 3;
    const locked = @atomicLoad(Size, &word, .unordered);
    if (!locked.resizing or locked.size != 42) return 4;
    @atomicStore(Size, &word, .{ .resizing = false, .size = 99 }, .release);
    if (word.resizing or word.size != 99) return 5;
    if (@atomicRmw(Size, &word, .Xor, .{ .resizing = true }, .seq_cst).size != 99 or !word.resizing) return 6;
    const reset = @atomicRmw(Size, &word, .Xchg, .{ .resizing = false }, .acq_rel);
    if (!reset.resizing or reset.size != 99 or word.resizing or word.size != 0) return 7;
    const Small = packed struct(u8) { low: u4, high: u4 };
    var byte: Small = .{ .low = 7, .high = 15 };
    const previous = @atomicRmw(Small, &byte, .And, .{ .low = 3, .high = 5 }, .monotonic);
    if (previous.low != 7 or previous.high != 15 or byte.low != 3 or byte.high != 5) return 8;
    return 42;
}

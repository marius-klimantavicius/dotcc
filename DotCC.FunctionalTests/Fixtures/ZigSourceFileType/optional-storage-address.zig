const Item = struct { value: u32, pointer: ?*u32 };
var calls: u8 = 0;
var slot: ?Item = null;
fn address() *?Item { calls += 1; return &slot; }
fn initialize(target: *?Item) void {
    target.* = .{ .value = 42, .pointer = &target.*.?.value };
}
pub fn main() u8 {
    initialize(&slot);
    const saved = slot.?.pointer orelse return 1;
    if (saved.* != 42 or saved != &slot.?.value) return 2;
    const projected = &address().*.?.value;
    if (calls != 1 or projected != saved) return 3;
    projected.* = 19;
    slot.?.value += 3;
    if (slot.?.value != 22 or saved.* != 22) return 4;
    const read = slot.?.value;
    if (read != 22 or calls != 1) return 5;
    return 42;
}

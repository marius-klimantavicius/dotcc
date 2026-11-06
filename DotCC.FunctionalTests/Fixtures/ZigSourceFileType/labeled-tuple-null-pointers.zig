var hint: ?[*]align(16) u8 = null;
fn advance(choice: u8, pointer: [*]align(16) u8) u8 {
    const old, const next = blk: {
        if (choice == 0) break :blk .{ null, null };
        const old = @atomicLoad(@TypeOf(hint), &hint, .unordered);
        if (choice == 1) break :blk .{ old, null };
        const next: ?[*]align(16) u8 = pointer;
        break :blk .{ old, next };
    };
    const result = @cmpxchgStrong(@TypeOf(hint), &hint, old, next, .monotonic, .monotonic);
    if (result != null) return 1;
    if (choice == 2 and hint != pointer) return 2;
    if (choice != 2 and hint != null) return 3;
    return 42;
}
pub fn main() u8 {
    var buffer: [32]u8 align(16) = @splat(0);
    if (advance(0, &buffer) != 42) return 1;
    if (advance(1, &buffer) != 42) return 2;
    if (advance(2, &buffer) != 42) return 3;
    if (advance(1, &buffer) != 42) return 4;
    if (advance(2, &buffer) != 42) return 5;
    return 42;
}

const Child = struct { pending: u64 };
const Parent = struct { count: u64, child: Child, probes: u64, tail: u64 };
fn field(parent: *Parent) *Child {
    parent.probes += 1;
    return &parent.child;
}
fn mutate(child: *Child, amount: u64) void {
    const parent: *Parent = @alignCast(@fieldParentPtr("child", child));
    parent.count += child.pending + amount;
    child.pending = 0;
    const count = &parent.count;
    count.* +%= 2;
    const same: *Parent = @fieldParentPtr("count", count);
    same.tail += 1;
}
fn check() u8 {
    var parent: Parent = .{ .count = 7, .child = .{ .pending = 2 }, .probes = 0, .tail = 11 };
    const pointer = field(&parent);
    const alias = pointer;
    if (pointer != alias) return 1;
    mutate(alias, 5);
    var snapshot = parent;
    mutate(field(&snapshot), 3);
    if (parent.count != 16 or parent.child.pending != 0 or parent.probes != 1 or parent.tail != 12) return 2;
    if (snapshot.count != 21 or snapshot.child.pending != 0 or snapshot.probes != 2 or snapshot.tail != 13) return 3;
    return 42;
}
const computed: u8 = computed: {
    break :computed check();
};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (computed != 42) return 4;
    return check();
}

const imported = @import("local-self-list.zig");
const Outer = struct {
    const Value = u8;
    fn verify() bool {
        const Entry = struct { value: Value, next: ?*@This() = null };
        var entry: Entry = .{ .value = 42 };
        entry.next = &entry;
        return entry.next.?.value == 42;
    }
};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const Node = struct { value: u32, prev: ?*@This() = null, next: ?*@This() = null };
    var first: Node = .{ .value = 1 };
    var second: Node = .{ .value = 2 };
    var middle: Node = .{ .value = 3 };
    if (@TypeOf(first.prev) != ?*Node or @TypeOf(first.next) != ?*Node) return 1;
    const List = imported.List(Node);
    var list: List = .{ .first = &first };
    first.next = &second;
    second.prev = &first;
    list.insertBefore(&second, &middle);
    if (first.next.? != &middle or middle.next.? != &second) return 2;
    if (second.prev.? != &middle or middle.prev.? != &first) return 3;
    if (!Outer.verify()) return 4;
    return 42;
}

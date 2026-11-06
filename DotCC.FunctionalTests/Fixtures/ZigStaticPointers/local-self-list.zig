pub fn List(comptime T: type) type {
    return struct {
        const Self = @This();
        pub const Node = T;
        first: ?*Node = null,
        pub fn insertBefore(list: *Self, node: *Node, new_node: *Node) void {
            new_node.next = node;
            if (node.prev) |previous| {
                new_node.prev = previous;
                previous.next = new_node;
            } else {
                new_node.prev = null;
                list.first = new_node;
            }
            node.prev = new_node;
        }
    };
}

var reads: u8 = 0;
var subjects: u8 = 0;
const Ref = struct {
    start: usize, end: usize,
    fn slice(self: Ref, memory: []const u8) []const u8 {
        reads += 1;
        return memory[self.start..self.end];
    }
};
const Id = union(enum) { explicit: Ref, implicit: u32 };
fn selected(id: Id) Id { subjects += 1; return id; }
fn copy(id: Id, memory: []const u8) ?[]const u8 {
    const id_: ?[]const u8 = switch (selected(id)) {
        .explicit => |value| value.slice(memory),
        .implicit => null,
    };
    return id_;
}
pub fn main() u8 {
    const memory = [_]u8{ 'a', 'b', 'c', 'd' };
    const content = copy(.{ .explicit = .{ .start = 1, .end = 3 } }, &memory) orelse return 1;
    if (content.len != 2 or content[0] != 'b' or content[1] != 'c') return 2;
    if (content.ptr != memory[1..3].ptr) return 3;
    const empty = copy(.{ .explicit = .{ .start = 2, .end = 2 } }, &memory) orelse return 4;
    if (empty.len != 0 or empty.ptr != memory[2..2].ptr) return 5;
    if (copy(.{ .implicit = 99 }, &memory) != null) return 6;
    if (subjects != 3 or reads != 2) return 7;
    return 42;
}

const Entry = struct { name: []const u8, value: u15, question: bool = false };
const entries = [_]Entry{
    .{ .name = "first", .value = 7 },
    .{ .name = "answer", .value = 42, .question = true },
};
const Tag = packed struct(u16) { value: u15, question: bool };
const E = tags: {
    var names: [entries.len][]const u8 = undefined;
    var values: [entries.len]u16 = undefined;
    for (entries, &names, &values) |entry, *name, *value| {
        name.* = entry.name;
        value.* = @bitCast(Tag{ .value = entry.value, .question = entry.question });
    }
    break :tags @Enum(u16, .exhaustive, &names, &values);
};
pub fn main() u8 {
    const tag: Tag = @bitCast(@backingInt(E.answer));
    return if (tag.question) @intCast(tag.value) else 0;
}

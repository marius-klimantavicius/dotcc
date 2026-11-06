comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const offset: u8 = 5;
    const Choice = enum(u8) {
        first = 9,
        second = 21,
        const Self = @This();
        pub fn score(self: Self) u8 {
            if (@typeInfo(@This()) != .@"enum") return 1;
            return switch (self) { .first => offset + 1, .second => offset + 2 };
        }
    };
    const Value = union(enum) {
        number: i32,
        text: []const u8,
        pub fn equal(self: @This(), other: @This()) bool {
            return switch (self) {
                .number => |left| switch (other) {
                    .number => |right| @mod(left, 10) == @mod(right, 10),
                    else => false,
                },
                .text => |left| switch (other) {
                    .text => |right| left.len == right.len,
                    else => false,
                },
            };
        }
    };
    const choice: Choice = .first;
    if (choice.score() != 6 or Choice.score(.second) != 7) return 1;
    if (@intFromEnum(choice) != 9) return 2;
    if (!Value.equal(.{ .number = 5 }, .{ .number = 25 })) return 3;
    if (Value.equal(.{ .number = 5 }, .{ .number = 6 })) return 4;
    const value: Value = .{ .text = "abc" };
    if (!value.equal(.{ .text = "xyz" }) or value.equal(.{ .text = "ab" })) return 5;
    return 42;
}

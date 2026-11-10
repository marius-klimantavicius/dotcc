fn Table(comptime T: type, comptime keys: []const T) type {
    return struct {
        pub fn len() usize { return keys.len; }
        pub fn get(index: usize) T { return keys[index]; }
    };
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const Empty = Table(u16, &.{});
    if (Empty.len() != 0) return 1;
    const Literal = Table(u16, &.{ 17, 42 });
    if (Literal.len() != 2 or Literal.get(1) != 42) return 2;
    const keys: []const u16 = &.{ 21, 42 };
    const keys_address = &keys;
    if (keys_address != &keys) return 7;
    const Named = Table(u16, keys);
    if (Named.len() != 2 or Named.get(0) != 21 or Named.get(1) != 42) return 3;
    const Other = Table(u16, &.{ 17, 23 });
    if (Other.get(1) != 23 or Literal.get(1) != 42) return 5;
    const Tail = Table(u16, keys[1..]);
    if (Tail.len() != 1 or Tail.get(0) != 42) return 6;
    const lifted = comptime result: {
        var buffer: [2]u16 = .{ 17, 0 };
        buffer[1] = 42;
        break :result buffer;
    };
    const Lifted = Table(u16, &lifted);
    if (Lifted.len() != 2 or Lifted.get(0) != 17 or Lifted.get(1) != 42) return 4;
    const typed: [2]u16 = lifted;
    const Typed = Table(u16, &typed);
    if (Typed.len() != 2 or Typed.get(0) != 17 or Typed.get(1) != 42) return 8;
    return 42;
}

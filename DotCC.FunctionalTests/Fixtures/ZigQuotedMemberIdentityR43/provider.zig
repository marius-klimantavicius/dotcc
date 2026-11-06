fn Make() type {
    return @Enum(u8, .exhaustive,
        &.{ "_256", "256", "a-b", "a?b", "__zig_member_612D62", "caf\xc3\xa9" },
        &.{ 2, 5, 9, 11, 13, 17 });
}
pub const Key = Make();

const CTag = enum(c_int) { answer = 42 };
const ZigTag = enum(u8) { answer = 1 };

fn Enum(use_c: bool, keys: anytype) type {
    _ = keys;
    return if (use_c) CTag else ZigTag;
}

const Tag = Enum(true, .{ "answer" });

fn answer(value: Tag) u8 {
    return if (value == .answer) 42 else 1;
}

pub fn main() u8 {
    return answer(.answer);
}

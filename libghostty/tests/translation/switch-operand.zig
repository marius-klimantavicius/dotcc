fn selected(value: u8) bool {
    return false or switch (value) {
        1, 2 => true,
        else => false,
    };
}

fn add(left: u32, right: u32) u32 {
    return left + right;
}

pub fn main() u8 {
    const sum = add(switch (@as(u8, 3)) {
        3 => @as(u32, 20),
        else => @as(u32, 10),
    }, 22);
    if (selected(1) and !selected(3) and sum == 42) {
        return 42;
    }
    return 1;
}

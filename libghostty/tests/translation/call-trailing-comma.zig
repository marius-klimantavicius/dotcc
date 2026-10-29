const support = @import("call-trailing-comma-support.zig");

fn twice(value: u32) u32 {
    return value * 2;
}

fn combine(left: u32, right: u32) u32 {
    return left + right;
}

pub fn main() u8 {
    const first = twice(9,);
    const second = combine(
        support.add(10, 2,),
        @as(u32, 12,),
    );
    return @intCast(first + second,);
}

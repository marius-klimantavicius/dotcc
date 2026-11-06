comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const blank: [40]u8 = @splat(' ');
    const address: *const [40]u8 = &blank;
    const text = blank ++ "0123456789abcdef0" ++ "xy";
    if (text.len != 59 or text[39] != ' ' or text[40] != '0' or
        text[56] != '0' or text[57] != 'x' or text[58] != 'y') return 1;
    if (address != &blank or address.*[0] != ' ' or address.*[39] != ' ') return 2;
    const octets: [4]u8 = .{ 0, 255, 1, 0 };
    const left = octets ++ "Z";
    const right = "A" ++ octets;
    if (left.len != 5 or left[0] != 0 or left[1] != 255 or left[3] != 0 or left[4] != 'Z') return 3;
    if (right.len != 5 or right[0] != 'A' or right[1] != 0 or right[2] != 255 or right[4] != 0) return 4;
    const terminated: [*:0]const u8 = left;
    if (terminated[5] != 0) return 5;
    return 42;
}

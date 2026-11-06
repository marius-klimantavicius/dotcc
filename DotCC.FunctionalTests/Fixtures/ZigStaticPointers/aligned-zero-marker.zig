const Footer = struct {
    checksum: u32,
    modify: u16,
    data: u16,
    _: void align(@max(8, @alignOf(usize))) = {},
};
const Wrapper = struct { footer: Footer, tail: u64 };
const Middle = struct { a: u8, marker: void align(8) = {}, b: u8 };
const AlignedValue = extern struct { prefix: u8, payload: u16 align(8), tail: u8 };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (@sizeOf(void) != 0 or @alignOf(Footer) != 8 or @sizeOf(Footer) != 8) return 1;
    if (@offsetOf(Footer, "checksum") != 0 or @offsetOf(Footer, "modify") != 4 or @offsetOf(Footer, "data") != 6) return 2;
    if (@offsetOf(Footer, "_") != 0) return 3;
    if (@alignOf(Wrapper) != 8 or @sizeOf(Wrapper) != 16 or @offsetOf(Wrapper, "footer") != 0 or @offsetOf(Wrapper, "tail") != 8) return 4;
    if (@alignOf(Middle) != 8 or @sizeOf(Middle) != 8 or @offsetOf(Middle, "marker") != 0 or @offsetOf(Middle, "b") != 1) return 5;
    if (@alignOf(AlignedValue) != 8 or @sizeOf(AlignedValue) != 16 or @offsetOf(AlignedValue, "payload") != 8 or @offsetOf(AlignedValue, "tail") != 10) return 6;
    var array: [2]Footer = undefined;
    array[0].checksum = 17;
    array[1].checksum = 42;
    if (@intFromPtr(&array[0]) % 8 != 0 or @intFromPtr(&array[1]) - @intFromPtr(&array[0]) != 8) return 7;
    return if (array[0].checksum == 17 and array[1].checksum == 42) 42 else 8;
}

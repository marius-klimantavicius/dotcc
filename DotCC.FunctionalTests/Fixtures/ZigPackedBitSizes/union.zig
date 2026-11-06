const GlobalRGB = packed struct(u24) { r: u8, g: u8, b: u8 };
const Data = packed union {
    rgb: GlobalRGB,
    codepoint: packed struct(u24) { data: u21, pad: u3 = 0 },
    palette: packed struct(u24) { data: u8, pad: u16 = 0 },
};
const Cell = packed struct(u64) {
    tag: u2 = 1,
    data: packed union {
        rgb: RGB,
        codepoint: packed struct(u24) { data: u21, pad: u3 = 0 },
        palette: packed struct(u24) { data: u8, pad: u16 = 0 },
    },
    rest: u38 = 0,
    pub const RGB = packed struct { r: u8, g: u8, b: u8 };
};

pub fn main() u8 {
    if (@bitSizeOf(Data) != 24 or @sizeOf(Data) != 4 or @alignOf(Data) != 4) return 1;
    if (@bitSizeOf(Cell) != 64 or @sizeOf(Cell) != 8 or @alignOf(Cell) != 8) return 2;
    var cells: [2]Cell = .{
        .{ .data = .{ .rgb = .{ .r = 0x12, .g = 0x34, .b = 0x56 } } },
        .{ .tag = 2, .data = .{ .palette = .{ .data = 0xA5 } } },
    };
    if (@as(u64, @bitCast(cells[0])) != 0x158D049) return 3;
    cells[0].data.rgb.g = 0xAB;
    if (@as(u64, @bitCast(cells[0])) != 0x15AAC49) return 4;
    if (@as(u64, @bitCast(cells[1])) != 0x296) return 5;
    cells[1].data.codepoint.data = 0x12345;
    if (@as(u64, @bitCast(cells[1])) != 0x48D16) return 6;
    const bytes: [*]const u8 = @ptrCast(&cells);
    if (bytes[0] != 0x49 or bytes[1] != 0xAC or bytes[8] != 0x16 or bytes[9] != 0x8D) return 7;
    return 42;
}

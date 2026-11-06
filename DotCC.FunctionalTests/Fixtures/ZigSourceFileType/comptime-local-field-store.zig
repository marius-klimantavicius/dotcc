const Capacity = struct { cols: u16, rows: u16 };
fn width() u16 { return 65535; }
const original: Capacity = .{ .cols = 80, .rows = 24 };
pub fn main() u8 {
    comptime {
        var capacity = original;
        capacity.cols = width();
        if (capacity.cols != 65535 or capacity.rows != 24) @compileError("field store lost");
    }
    if (original.cols != 80 or original.rows != 24) return 1;
    return 42;
}

const Row = struct { value: u8 };
const Table = struct { bytes: []const u8 };
inline fn make(comptime rows: anytype) Table {
    comptime {
        var out: [rows[0].value]u8 = undefined;
        for (&out) |*byte| byte.* = rows[1].value;
        const final = out;
        return .{ .bytes = &final };
    }
}
const first = [_]Row{ .{ .value = 3 }, .{ .value = 7 } };
const second = [_]Row{ .{ .value = 2 }, .{ .value = 14 } };
pub fn main() u8 {
    const a = make(first);
    const b = make(second);
    const c = make(rows: {
        if (!@inComptime()) unreachable;
        var rows = first;
        rows[0].value = 1;
        rows[1].value = 42;
        break :rows rows;
    });
    if (a.bytes.len != 3 or b.bytes.len != 2) return 1;
    if (a.bytes[0] != 7 or a.bytes[2] != 7 or b.bytes[1] != 14) return 2;
    if (c.bytes.len != 1 or c.bytes[0] != 42) return 3;
    return a.bytes[0] + a.bytes[1] + a.bytes[2] + b.bytes[0] + 7;
}

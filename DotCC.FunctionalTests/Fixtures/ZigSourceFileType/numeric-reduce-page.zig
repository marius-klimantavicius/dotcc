// page.zig:2551: Row dirty scan, eight packed u64 values loaded by bitcast.
const Row = packed struct(u64) {
    cells: u32 = 0,
    wrap: bool = false,
    wrap_continuation: bool = false,
    grapheme: bool = false,
    styled: bool = false,
    hyperlink: bool = false,
    semantic_prompt: u2 = 0,
    kitty_virtual_placeholder: bool = false,
    dirty: bool = false,
    padding: u23 = 0,
};
const Group = @Vector(8, u64);
fn load(values: []const Row, i: usize, calls: *u32) Group {
    calls.* += 1;
    return @bitCast(values[i..][0..8].*);
}
fn match(values: []const Row, i: usize, calls: *u32) bool {
    return @reduce(.Or, load(values, i, calls)) & (@as(u64, 1) << @bitOffsetOf(Row, "dirty")) == 0;
}
pub fn main() u8 {
    var calls: u32 = 0;
    var rows: [10]Row = @splat(.{});
    rows[0].dirty = true;
    rows[9].dirty = true;
    if (!match(&rows, 1, &calls) or calls != 1) return 1;
    for (1..9) |lane| {
        rows[lane].cells = @intCast(lane);
        rows[lane].padding = 0x400000;
        if (!match(&rows, 1, &calls)) return 2;
        rows[lane].dirty = true;
        if (match(&rows, 1, &calls)) return 3;
        rows[lane].dirty = false;
    }
    if (calls != 17) return 4;
    const raw = @reduce(.Or, load(&rows, 1, &calls));
    if (raw != 0x800000000000000f or calls != 18) return 5;
    return 42;
}
